# protomap-test-csharp

A synthetic C# code base for testing [ProtoMap](https://protomap.ai) by [HoundDog.ai](https://hounddog.ai). It is part
of the code of Acme, a fictional company.

The code mixes in, on purpose, what makes static analysis of real code bases hard, and `expected.json` is the answer key
of what a scanner should find in it.

Acme's code is spread over multiple repositories, one per programming language. They were generated together from one
catalog of Acme's APIs. Each owns its own domains and calls the others' services, so it holds copies of the protobuf
files it needs from them, byte for byte as their owners have them. Scan them together to follow calls and data from one
repository to another.

| Repository                                                                         | Language   |
|------------------------------------------------------------------------------------|------------|
| **protomap-test-csharp** (this one)                                                | C#         |
| [protomap-test-golang](https://github.com/hounddogai/protomap-test-golang)         | Go         |
| [protomap-test-java](https://github.com/hounddogai/protomap-test-java)             | Java       |
| [protomap-test-javascript](https://github.com/hounddogai/protomap-test-javascript) | JavaScript |
| [protomap-test-kotlin](https://github.com/hounddogai/protomap-test-kotlin)         | Kotlin     |
| [protomap-test-python](https://github.com/hounddogai/protomap-test-python)         | Python     |
| [protomap-test-rust](https://github.com/hounddogai/protomap-test-rust)             | Rust       |
| [protomap-test-swift](https://github.com/hounddogai/protomap-test-swift)           | Swift      |
| [protomap-test-typescript](https://github.com/hounddogai/protomap-test-typescript) | TypeScript |

## Repository Layout

A .NET 10 solution:

- `src/Domains`: the domain modules. Each mixes endpoint classes, partial feature classes, ports, adapters, and domain
  record mappers.
- `src/Host`: the ASP.NET Core gRPC endpoints.
- `src/Platform`: Dapper repositories, identity interceptors, typed client registration, retry policies, mapping
  profiles, and notification workers.
- `src/Programs`: the directory sweep, the change indexer, and the operator CLI.
- `src/Contracts`: compiles the protobuf files in `proto/` with Grpc.Tools into `obj/`. Generated bindings are not
  committed.
- `checks`: executable checks of the handlers.

`PaymentCase` properties adapt the settlement and funding oneofs without changing the wire contracts.

## Build and Run

Build with the .NET 10 SDK, then run the checks:

```sh
dotnet build Acme.Workforce.slnx -c Release -m:2
dotnet run --project checks -c Release
```

The host reads `Mesh:Address`, `Mesh:ServiceToken`, and `ConnectionStrings:Workforce` from its configuration.

## Added Code and Benchmark Labels

`tools/grow` (seed 20261116) added ordinary application code to this repository, and 16 programs whose jobs carry rpc
responses to other rpcs through constructs a static analysis cannot resolve. All of it is under `src/Kit/`,
`src/Domains/Compensation/`, `src/Domains/Talent/`, `src/Domains/Workplace/`, `src/Domains/Absence/`,
`src/Domains/Payrollops/`, `src/Domains/Assets/`, `src/Domains/Engagement/`, `src/Domains/Compliance/`,
`src/Domains/Contractors/`, `src/Domains/Orgdesign/`, `src/Programs/Ops/`.

- The ordinary code (domain models, validation rules, stores and caches, adjustment engines, reports, workflows, feature
  flags, retries, command-line tools, and unit tests) makes no gRPC calls, so it adds no flows.
- Each planted job reads one rpc's response and hands some of its fields to one or more implementations, each calling
  another rpc. Which implementations run is decidable from the code of the functions on the flow's path. A flow through
  one that runs is in `flows`, with a `pattern` ending in `_real`, or the plain pattern name when every implementation
  runs; one through an implementation that cannot run is in `forbidden_flows`, with the full source and target and a
  pattern ending in `_decoy`.
- `benchmark` in `expected.json` names the generator, its seed, the new patterns, the new code's path and function
  prefixes, and the planted programs' islands. The labels are complete for the new code: every real flow it creates is
  in `flows`, so a flow the scanner reports in the new code that is not there is false.
- New `flows` entries are `known_gap` unless the scanner already finds them. Unlike the other `forbidden_flows`, which
  only name a source, the `_decoy` entries name an endpoint pair; `fixture_coverage.rs`, which reads only their source
  and target kind, lists the ones the scanner reports.

| Pattern | What decides which implementation runs | True | False |
|---|---|---:|---:|
| `direct_call` | nothing: one concrete call, which the scanner finds certain | 95 | 0 |
| `dispatch_closure_map` | a map of closures, one called by a constant key | 90 | 169 |
| `dispatch_filter_loop` | a loop over keyed implementations that skips all but one key | 77 | 147 |
| `dispatch_func_value` | a function value a switch on a constant kind reassigns | 68 | 118 |
| `dispatch_index` | a list of implementations, one taken by a constant index | 56 | 125 |
| `dispatch_registry` | a registry of implementations looked up by a constant key | 104 | 222 |
| `dispatch_runtime` | run-time data, so each implementation may run | 71 | 0 |
| `dispatch_switch` | a selector's switch on a kind its caller passes as a constant | 117 | 212 |
| `fanout_all` | nothing: a loop runs every implementation | 73 | 0 |
