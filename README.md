Experiments with [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor) WASM.

Snake game: https://iseiryu.github.io/blazor/snake
Snake game AOT: https://iseiryu.github.io/blazor-aot/snake

### Build and Run

```bash
cd /repo/folder

dotnet workload install wasm-tools
dotnet workload restore
dotnet build
dotnet run --project .\BlazorExperiments\BlazorExperiments.UI.csproj
```
