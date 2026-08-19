# Northwind.StaticSite

The `platform/static-site` archetype: a pre-built bundle synced to the site bucket the `static-site`
Terraform module creates.

`identity.json` is a placeholder in the repository and is **overwritten by the deployment step** with
the component, environment, stamp and spec version actually being deployed. That is the static
equivalent of the `/identity` endpoint the web service serves — without it, a bundle in a bucket cannot
report which spec version it is, and observed state for this archetype could only ever be "some files
exist".
