using System;
using System.Net.Http;
using BlazorStyled;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorTests;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddBlazorStyled();
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();