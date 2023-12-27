using BlazorExperiments.UI.Models;
using Microsoft.JSInterop;

namespace BlazorExperiments.UI.Services;

public static class BrowserResizeService {
    public static WindowProperties? CurrentWindowProperties { get; private set; }
    public static event Func<Task>? OnResize;

    [JSInvokable]
    public static async Task OnBrowserResize() {
        if (OnResize is not null)
            await OnResize.Invoke();
    }

    public static async ValueTask<WindowProperties> GetWindowProperties(IJSRuntime jSRuntime) {
        CurrentWindowProperties = await jSRuntime.InvokeAsync<WindowProperties>("browserResize.getWindowDimensions");
        return CurrentWindowProperties;
    }
}