# Test.Automated

Touchstone console runner for OpenAuditLog. It executes the shared test descriptors defined in
`Test.Shared` (`OpenAuditLogSuites.All`) and prints a readable pass/fail summary without requiring
`dotnet test`. The process exit code is zero only when every executed test passes.

## Run

```
dotnet run --project src/Test.Automated/Test.Automated.csproj
```

## Filter

```
dotnet run --project src/Test.Automated/Test.Automated.csproj -- --suite emitter
dotnet run --project src/Test.Automated/Test.Automated.csproj -- --test round_trip
```

- `--suite <text>` runs only suites whose id contains `<text>` (case-insensitive).
- `--test <text>` runs only tests whose id contains `<text>` (case-insensitive).
- `--help` shows usage.

The same descriptors are also hosted by `Test.Xunit` (xUnit) and `Test.Nunit` (NUnit) so they can be
run through `dotnet test` and standard CI tooling.
