# Northwind API contracts

One directory per API in the Lembos sample catalog, one file per published `ApiSpecVersion`, named for
its version tag. The path is the API's qualified name, so `ordering/orders-api` version `3.2.0` is
`specs/ordering/orders-api/3.2.0.yaml`.

| API | Type | Versions |
| --- | --- | --- |
| `ordering/orders-api` | OpenAPI | 3.1.0, 3.2.0 |
| `ordering/order-events` | AsyncAPI | 2.0.0 |
| `checkout/checkout-api` | OpenAPI | 1.8.0 |
| `payments/payments-api` | OpenAPI | 4.1.0 |
| `payments/payment-events` | AsyncAPI | 1.3.0 |
| `merchandising/products-api` | OpenAPI | 5.0.0 |
| `merchandising/product-search-api` | GraphQL | 0.9.0 |
| `merchandising/pricing-api` | gRPC | 2.2.0 |
| `warehouse/inventory-api` | gRPC | 3.0.0 |
| `delivery/dispatch-api` | OpenAPI | 0.7.0 |
| `accounts/customers-api` | OpenAPI | 2.5.0 |
| `accounts/legacy-profile-api` | SOAP | 1.0.0 |
| `support/tickets-api` | OpenAPI | 0.4.0 |
| `platform/catalog-graph-api` | GraphQL | 0.2.0 |

**`orders-api` carries two versions on purpose.** An `Api` whose whole history is one version cannot
demonstrate the thing the model exists for — a consumer integrated against 3.1.0 needs to see what it
agreed to after 3.2.0 lands. 3.2.0 adds `cancellationReason` and deprecates `Order.notes`; the diff is
small and visible, which is what makes it useful as a fixture.

## How these reach the platform

Both fields on `ApiSpecVersion` are populated, because they answer different questions:

- **`Content`** — the document inline, so the Portal renders without a network call and works offline.
- **`Url`** — the source, so "open the source document" goes somewhere real:
  `https://raw.githubusercontent.com/lembosproj-northwind/apps/main/specs/{api}/{version}.{ext}`

`raw.githubusercontent.com` serves `Access-Control-Allow-Origin: *`, which the browser-side explorer
needs; a spec host without it renders an empty frame while the URL is perfectly correct.
