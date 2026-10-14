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
