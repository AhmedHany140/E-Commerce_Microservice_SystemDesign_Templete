# Local service ports

| Service | HTTP | HTTPS | Gateway path |
|---|---:|---:|---|
| Auth | 5051 | 7088 | `/api/auth/*` |
| Cart | 5057 | 7209 | `/api/cart/*` |
| Product | 5090 | 7103 | `/api/products/*` and `/graphql` |
| Order | 5072 | 7046 | `/api/orders/*` and `/graphql` |
| Payment | 5113 | 7053 | `/api/payment/*` |
| Email | 5103 | 7245 | internal only |
| Query Gateway | 5008 | 7206 | `/graphql` through Command Gateway |
| Command Gateway | 5046 | 7225 | public entry point |

The default appsettings keep RabbitMQ disabled so HTTP/gRPC APIs can start without secrets. Put the values once in a repository-level `.env`, then run `./run-local.ps1 -WithRabbitMq` when RabbitMQ is available.
