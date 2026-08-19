# Northwind Apps

The archetype workloads the Lembos sample organisation deploys. **Five apps cover all 35 seeded
components**, because a component is not a distinct program — it is an instance of a blueprint, and
there are five component blueprints:

| App | Blueprint | Artifact |
| --- | --- | --- |
| `Northwind.WebService` | `platform/helm-web-service` | `ghcr.io/lembosproj-northwind/apps/web-service` |
| `Northwind.Worker` | `platform/helm-worker` | `ghcr.io/lembosproj-northwind/apps/worker` |
| `Northwind.CronJob` | `platform/helm-cron-job` | `ghcr.io/lembosproj-northwind/apps/cron-job` |
| `Northwind.Function` | `platform/serverless-function` | a zip in the artifacts bucket |
| `Northwind.StaticSite` | `platform/static-site` | assets synced to the site bucket |

That is the point rather than a shortcut. A platform does not care what a workload computes; it cares
which blueprint built it, which spec version it is running, and whether what is running matches what was
asked for. Thirty-five bespoke programs would demonstrate none of that better than five.

## Why these report their own identity

Every app reads `LEMBOS_COMPONENT`, `LEMBOS_ENVIRONMENT`, `LEMBOS_STAMP` and `LEMBOS_SPEC_VERSION` from
its environment and reports them back — the web service on `/identity`, the others in structured logs.

**This is what makes drift observable.** `RuntimeInstance` records a desired state and an observed one,
and the gap between them is the platform's actual work. Without a workload that reports which spec
version it is actually running, observed state can only ever be inferred from the fact that a pod
exists — which cannot tell a successful deployment from a stale one that was never replaced.

`/readyz` fails while a required resource binding is unresolvable, so an instance that is running but
cannot reach its database reports unhealthy rather than ready. Health and drift are different axes, and
this is the app half of keeping them apart.

## Resource bindings

A workload never receives a credential as a value. It receives `<HANDLE>_URL` from a Kubernetes secret
the provisioning run placed, or `<HANDLE>_SECRET_REF` naming a path to resolve at start-up. Both spellings
are handled; which one arrives depends on the provisioner.
