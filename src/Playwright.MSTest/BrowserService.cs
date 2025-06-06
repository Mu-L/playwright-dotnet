/*
 * MIT License
 *
 * Copyright (c) Microsoft Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Playwright.TestAdapter;

namespace Microsoft.Playwright.MSTest;

internal class BrowserService : IWorkerService
{
    public IBrowser Browser { get; private set; }

    private BrowserService(IBrowser browser)
    {
        Browser = browser;
    }

    public static Task<BrowserService> Register(WorkerAwareTest test, IBrowserType browserType, (string, BrowserTypeConnectOptions?)? connectOptions)
    {
        return test.RegisterService("Browser", async () => new BrowserService(await CreateBrowser(browserType, connectOptions).ConfigureAwait(false)));
    }

    private static async Task<IBrowser> CreateBrowser(IBrowserType browserType, (string WSEndpoint, BrowserTypeConnectOptions? Options)? connectOptions)
    {
        if (connectOptions.HasValue && connectOptions.Value.WSEndpoint != null)
        {
            var options = new BrowserTypeConnectOptions(connectOptions?.Options ?? new());
            var headers = options.Headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [];
            headers.Add("x-playwright-launch-options", JsonSerializer.Serialize(PlaywrightSettingsProvider.LaunchOptions, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
            options.Headers = headers;
            return await browserType.ConnectAsync(connectOptions!.Value.WSEndpoint, options).ConfigureAwait(false);
        }

        var legacyBrowser = await ConnectBasedOnEnv(browserType);
        if (legacyBrowser != null)
        {
            return legacyBrowser;
        }
        return await browserType.LaunchAsync(PlaywrightSettingsProvider.LaunchOptions).ConfigureAwait(false);
    }

    // TODO: Remove at some point
    private static async Task<IBrowser?> ConnectBasedOnEnv(IBrowserType browserType)
    {
        var accessToken = Environment.GetEnvironmentVariable("PLAYWRIGHT_SERVICE_ACCESS_TOKEN");
        var serviceUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_SERVICE_URL");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(serviceUrl))
        {
            return null;
        }

        var exposeNetwork = Environment.GetEnvironmentVariable("PLAYWRIGHT_SERVICE_EXPOSE_NETWORK") ?? "<loopback>";
        var os = Uri.EscapeDataString(Environment.GetEnvironmentVariable("PLAYWRIGHT_SERVICE_OS") ?? "linux");
        var runId = Uri.EscapeDataString(Environment.GetEnvironmentVariable("PLAYWRIGHT_SERVICE_RUN_ID") ?? DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
        var apiVersion = "2023-10-01-preview";
        var wsEndpoint = $"{serviceUrl}?os={os}&runId={runId}&api-version={apiVersion}";

        return await browserType.ConnectAsync(wsEndpoint, new BrowserTypeConnectOptions
        {
            Timeout = 3 * 60 * 1000,
            ExposeNetwork = exposeNetwork,
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {accessToken}",
                ["x-playwright-launch-options"] = JsonSerializer.Serialize(PlaywrightSettingsProvider.LaunchOptions, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
            }
        }).ConfigureAwait(false);
    }

    public Task ResetAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Browser.CloseAsync();
}
