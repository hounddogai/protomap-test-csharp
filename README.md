# protomap-test-csharp

A synthetic code base of the fictional company Acme for testing ProtoMap: 340 gRPC services with 2614 methods in 17
domains, and 9700 protobuf messages with 42198 fields, many nested several levels deep.

It was generated, with the eight other `protomap-test-*` repositories, from one catalog of Acme's APIs. The repositories
own disjoint domains and call each other's services, so each holds copies of the protobuf files it needs from the
others, byte for byte as their owners have them.

## Layout

A .NET 10 solution with 17 domain modules under `src/Domains`, ASP.NET Core gRPC endpoints in `src/Host`, and Dapper
repositories, identity interceptors, typed client registration, retry policies, mapping profiles, and notification
workers in `src/Platform`. Each domain mixes endpoint classes, partial feature classes, ports, adapters, and domain
record mappers. `src/Programs` contains the directory sweep, change indexer, and operator CLI. `src/Contracts` compiles
the vendored `proto/` files with Grpc.Tools into `obj/`; generated bindings are not committed. Build with `dotnet build
Acme.Workforce.slnx -c Release -m:2`; run the executable checks with `dotnet run --project checks -c Release`. The host
reads `Mesh:Address`, `Mesh:ServiceToken`, and `ConnectionStrings:Workforce` from configuration. PaymentCase properties
adapt the catalog's settlement and funding oneofs without changing the wire contracts. The oracle keeps the first
nonoverlapping response assignment when catalog returns compete for a singular field; 459 conflicting return mappings
are omitted. All planned calls and request mappings are emitted. Only data crossing the background notification queue is
marked known_gap.

The domains are `employeeportal`, `documents`, `employees`, `recruiting`, `onboarding`, `timekeeping`, `scheduling`,
`benefits`, `learning`, `performance`, `workplaces`, `travel`, `corporateevents`, `helpdesk`, `itsm`, `devices`,
`licensing`. It calls services of `admissions`, `analytics`, `anomalies`, `appointments`, `assets`, `audiences`,
`audit`, `authorization`, `banking`, `boxoffice`, `broadcasts`, `budgeting`, `campaigns`, `careplans`, `cart`,
`catalog`, `certificates`, `chat`, `checkout`, `community`, `compliance`, `concessions`, `configuration`, `content`,
`customers`, `datahub`, `datasets`, `deliveries`, `dispatch`, `driverapp`, `drivers`, `edgegateway`, `experiments`,
`fanportal`, `featureflags`, `featurestore`, `fleet`, `forecasting`, `forms`, `fraud`, `fuel`, `giftcards`, `identity`,
`insurance`, `invoicing`, `keymanagement`, `labeling`, `ledger`, `lending`, `lineage`, `localization`, `loyalty`,
`maintenance`, `media`, `merchandising`, `messaging`, `metrics`, `modelregistry`, `notifications`, `orders`,
`patientapp`, `patients`, `payments`, `payroll`, `performers`, `personalization`, `pipelines`, `prescriptions`,
`pricing`, `procurement`, `promotions`, `ratelimiting`, `recommendations`, `returns`, `reviews`, `risk`, `routing`,
`search`, `seating`, `secrets`, `sellers`, `shipping`, `shows`, `sponsorships`, `subscriptions`, `suppliers`, `surveys`,
`telehealth`, `telematics`, `ticketing`, `tours`, `treasury`, `venues`, `warehousing`, `webbff`, `webhooks`, `yards`
too.

## Oracle

`expected.json` lists what scanning the repository must find: every handler, every call a handler or a program makes,
and every field-level flow from where a handler has data to where a call or its response takes it, each tagged with the
shape of the code that makes it. `crates/graph/tests/fixture_coverage.rs` in `protomap-source` checks a scan against it.

The code mixes in, on purpose, what makes static analysis of real code bases hard. Flows by pattern:

| Pattern | Flows |
|---|---|
| `any_envelope` | 1929 |
| `any_roundtrip` | 812 |
| `async_chain` | 822 |
| `call_invoker` | 924 |
| `client_stream_call` | 30 |
| `constructor_delegate` | 856 |
| `duplex_call` | 16 |
| `echo` | 3894 |
| `extension_builder` | 843 |
| `foreach_each` | 430 |
| `json_bytes` | 811 |
| `json_roundtrip` | 783 |
| `linq_projection` | 800 |
| `mapping_profile` | 805 |
| `method_group` | 895 |
| `notification_worker` | 319 |
| `oneof_switch` | 3914 |
| `partial_workflow` | 843 |
| `program_batch` | 6 |
| `program_stream` | 5 |
| `record_with` | 935 |
| `reflection_copy` | 799 |
| `registry_lookup` | 883 |
| `retry_policy` | 761 |
| `selectmany_each` | 320 |
| `server_stream_call` | 604 |
| `stepwise_request` | 777 |
| `task_when_all` | 1311 |
| `task_when_all_each` | 427 |
| `typed_client` | 843 |

Flows whose values pass through something on their way, by what:

| Transform | Flows |
|---|---|
| `any_pack` | 473 |
| `any_roundtrip` | 562 |
| `in_process_notification` | 319 |
| `json_bytes` | 200 |
| `json_roundtrip` | 550 |
| `oneof_switch` | 900 |
| `reflection_copy` | 564 |

Calls by pattern:

| Pattern | Calls |
|---|---|
| `any_envelope` | 329 |
| `any_roundtrip` | 178 |
| `async_chain` | 186 |
| `call_invoker` | 197 |
| `client_stream_call` | 8 |
| `constructor_delegate` | 188 |
| `duplex_call` | 5 |
| `extension_builder` | 183 |
| `foreach_each` | 114 |
| `json_bytes` | 135 |
| `json_roundtrip` | 169 |
| `linq_projection` | 170 |
| `mapping_profile` | 184 |
| `method_group` | 195 |
| `notification_worker` | 91 |
| `oneof_switch` | 742 |
| `partial_workflow` | 177 |
| `program_batch` | 6 |
| `program_cli` | 17 |
| `program_stream` | 10 |
| `record_with` | 203 |
| `reflection_copy` | 174 |
| `registry_lookup` | 187 |
| `retry_policy` | 168 |
| `selectmany_each` | 85 |
| `server_interceptor` | 2614 |
| `server_stream_call` | 180 |
| `stepwise_request` | 173 |
| `task_when_all` | 286 |
| `task_when_all_each` | 115 |
| `typed_client` | 184 |
