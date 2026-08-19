# Unique Id Generator

`UniqueIdGenerator` is a small .NET library for generating 64-bit, time-ordered unique IDs. It is inspired by Twitter's Snowflake algorithm and is useful when database-generated IDs, GUIDs, or a central ID service are not a good fit.

Each ID contains a millisecond timestamp, a generator ID, and a sequence number. This makes IDs sortable by creation time while allowing multiple processes to generate IDs independently.

## When to use it

Use this library when you need compact, roughly time-ordered IDs that can be generated locally at high throughput. Typical examples include database keys, messages, events, and distributed application records.

Do not use the same generator ID in more than one concurrently running process.

## Requirements

- .NET 8.0 or later
- One unique generator ID per process, in the range `0` through `511`
- One shared, fixed UTC start date for all generators that must produce globally unique IDs

The system clock must not move backwards. A backward clock adjustment can break the time ordering and may compromise uniqueness.

## Quick start

Install the package from [NuGet](https://www.nuget.org/packages/UniqueIdGenerator/), then create a generator with a unique generator ID and a shared start date:

```csharp
using UniqueIdGenerator.Net;

var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
var generator = new Generator(generatorId: 42, start: startDate);

// A 12-character Base64 representation of the 64-bit ID.
string id = generator.Next();

// The same ID format as an unsigned 64-bit integer, without allocations.
ulong numericId = generator.NextLong();
```

`Generator` instances are not thread-safe. Create one generator per concurrent worker, each with a distinct generator ID.

## ID layout

The built-in layout is:

| Part | Bits | Purpose |
| --- | ---: | --- |
| Timestamp | 42 | Milliseconds since the configured start date |
| Generator ID | 9 | Identifies one of 512 independent generators |
| Sequence | 13 | Orders up to 8,192 IDs created by one generator in the same millisecond |

The timestamp range covers approximately 139 years. The bit allocation is currently compiled into the library; changing it requires rebuilding the source and changes the ID format.

## Converting and inspecting IDs

Use `IdConverter` to convert between the Base64 and numeric representation:

```csharp
ulong numericId = IdConverter.ToLong(id);
string base64Id = IdConverter.ToString(numericId);
```

Use `IdParts` when you need to inspect an ID's timestamp, generator ID, sequence, or raw bit layout:

```csharp
var parts = new IdParts(id);
Console.WriteLine(parts.Time);
Console.WriteLine(parts.GeneratorId);
Console.WriteLine(parts.Sequence);
```

## Benchmarking

The repository contains BenchmarkDotNet benchmarks for the two generation APIs:

```powershell
dotnet run --project IdGenerator.net.Benchmarks\UniqueIdGenerator.Net.Benchmarks.csproj --configuration Release
```

Benchmark results depend on the hardware, operating system, runtime version, and system clock behavior. Run the benchmark on the target environment before relying on a throughput number.

## License

Developed by [Michael Schuler](https://www.michaelschuler.ch) under the [MIT License](LICENSE).
