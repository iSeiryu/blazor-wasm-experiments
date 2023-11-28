using BlazorExperiments.UI.Models;
using Microsoft.JSInterop;

namespace BlazorExperiments.UI.Services;

public static class BrowserResizeService {
    public static event Func<Task>? OnResize;

    [JSInvokable]
    public static async Task OnBrowserResize() {
        await OnResize?.Invoke();
    }

    public static async ValueTask<WindowProperties> GetWindowProperties(IJSRuntime jSRuntime) {
        return await jSRuntime.InvokeAsync<WindowProperties>("browserResize.getWindowDimensions");
    }
}