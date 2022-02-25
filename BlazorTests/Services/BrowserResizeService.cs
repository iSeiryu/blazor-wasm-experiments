using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using BlazorTests.Models;

namespace BlazorTests
{
    public class BrowserResizeService
    {
        public static event Func<Task> OnResize;

        [JSInvokable]
        public static async Task OnBrowserResize()
        {
            await OnResize?.Invoke();           
        }

        public static async Task<int> GetInnerHeight(IJSRuntime jSRuntime)
        {
            return await jSRuntime.InvokeAsync<int>("browserResize.getInnerHeight");
        }

        public static async Task<WindowDimensions> GetWindowDimensions(IJSRuntime jSRuntime)
        {
            return await jSRuntime.InvokeAsync<WindowDimensions>("browserResize.getWindowDimensions");
        }
    }
}