/*
 * MIT License
 *
 * Copyright (c) Microsoft Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright.Helpers;
using Microsoft.Playwright.Transport;
using Microsoft.Playwright.Transport.Protocol;

namespace Microsoft.Playwright.Core;

internal class Browser : ChannelOwner, IBrowser
{
    private readonly BrowserInitializer _initializer;
    private readonly TaskCompletionSource<bool> _closedTcs = new();
    internal readonly List<BrowserContext> _contexts = new();
    internal string? _tracesDir = null;
    internal BrowserType _browserType = null!;
    internal string? _closeReason;

    internal Browser(ChannelOwner parent, string guid, BrowserInitializer initializer) : base(parent, guid)
    {
        IsConnected = true;
        _initializer = initializer;
    }

    public event EventHandler<IBrowser>? Disconnected;

    public IReadOnlyList<IBrowserContext> Contexts => _contexts.ToArray();

    public bool IsConnected { get; private set; }

    internal bool ShouldCloseConnectionOnClose { get; set; }

    public string Version => _initializer.Version;

    public IBrowserType BrowserType => _browserType;

    internal override void OnMessage(string method, JsonElement serverParams)
    {
        switch (method)
        {
            case "context":
                DidCreateContext(serverParams.GetProperty("context").ToObject<BrowserContext>(_connection.DefaultJsonSerializerOptions)!);
                break;
            case "close":
                DidClose();
                break;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task CloseAsync(BrowserCloseOptions? options = default)
    {
        _closeReason = options?.Reason;
        try
        {
            if (ShouldCloseConnectionOnClose)
            {
                _connection.DoClose(null as string);
            }
            else
            {
                await SendMessageToServerAsync("close", new Dictionary<string, object?>
                {
                    ["reason"] = options?.Reason,
                }).ConfigureAwait(false);
            }
            await _closedTcs.Task.ConfigureAwait(false);
        }
        catch (Exception e) when (DriverMessages.IsTargetClosedError(e))
        {
            // Swallow exception
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<IBrowserContext> NewContextAsync(BrowserNewContextOptions? options = default)
    {
        options ??= new();

        var args = new Dictionary<string, object?>
        {
            ["bypassCSP"] = options.BypassCSP,
            ["deviceScaleFactor"] = options.DeviceScaleFactor,
            ["serviceWorkers"] = options.ServiceWorkers,
            ["geolocation"] = options.Geolocation,
            ["hasTouch"] = options.HasTouch,
            ["httpCredentials"] = options.HttpCredentials,
            ["ignoreHTTPSErrors"] = options.IgnoreHTTPSErrors,
            ["isMobile"] = options.IsMobile,
            ["javaScriptEnabled"] = options.JavaScriptEnabled,
            ["locale"] = options.Locale,
            ["offline"] = options.Offline,
            ["permissions"] = options.Permissions,
            ["proxy"] = options.Proxy,
            ["strictSelectors"] = options.StrictSelectors,
            ["colorScheme"] = options.ColorScheme == ColorScheme.Null ? "no-override" : options.ColorScheme,
            ["reducedMotion"] = options.ReducedMotion == ReducedMotion.Null ? "no-override" : options.ReducedMotion,
            ["forcedColors"] = options.ForcedColors == ForcedColors.Null ? "no-override" : options.ForcedColors,
            ["contrast"] = options.Contrast == Contrast.Null ? "no-override" : options.Contrast,
            ["extraHTTPHeaders"] = options.ExtraHTTPHeaders?.Select(kv => new HeaderEntry { Name = kv.Key, Value = kv.Value }).ToArray(),
            ["recordVideo"] = GetVideoArgs(options.RecordVideoDir, options.RecordVideoSize),
            ["timezoneId"] = options.TimezoneId,
            ["userAgent"] = options.UserAgent,
            ["baseURL"] = options.BaseURL,
            ["clientCertificates"] = ToClientCertificatesProtocol(options.ClientCertificates),
            ["selectorEngines"] = _browserType.Playwright._selectors._selectorEngines,
            ["testIdAttributeName"] = _browserType.Playwright._selectors._testIdAttributeName,
        };

        if (options.AcceptDownloads.HasValue)
        {
            args.Add("acceptDownloads", options.AcceptDownloads.Value ? "accept" : "deny");
        }

        var storageState = options.StorageState;
        if (!string.IsNullOrEmpty(options.StorageStatePath))
        {
            if (!File.Exists(options.StorageStatePath))
            {
                throw new PlaywrightException($"The specified storage state file does not exist: {options.StorageStatePath}");
            }

            storageState = File.ReadAllText(options.StorageStatePath);
        }

        if (!storageState.IsNullOrEmpty())
        {
            args.Add("storageState", JsonSerializer.Deserialize<object>(storageState, Helpers.JsonExtensions.DefaultJsonSerializerOptions));
        }

        if (options.ViewportSize?.Width == -1)
        {
            args.Add("noDefaultViewport", true);
        }
        else
        {
            args.Add("viewport", options.ViewportSize);
            args.Add("screen", options.ScreenSize);
        }

        var context = await SendMessageToServerAsync<BrowserContext>("newContext", args).ConfigureAwait(false);
        await context.InitializeHarFromOptionsAsync(options).ConfigureAwait(false);
        return context;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<IPage> NewPageAsync(BrowserNewPageOptions? options = default)
    {
        options ??= new();

        var contextOptions = new BrowserNewContextOptions()
        {
            AcceptDownloads = options.AcceptDownloads,
            IgnoreHTTPSErrors = options.IgnoreHTTPSErrors,
            BypassCSP = options.BypassCSP,
            ViewportSize = options.ViewportSize,
            ScreenSize = options.ScreenSize,
            UserAgent = options.UserAgent,
            DeviceScaleFactor = options.DeviceScaleFactor,
            IsMobile = options.IsMobile,
            HasTouch = options.HasTouch,
            JavaScriptEnabled = options.JavaScriptEnabled,
            TimezoneId = options.TimezoneId,
            Geolocation = options.Geolocation,
            Locale = options.Locale,
            Permissions = options.Permissions,
            ExtraHTTPHeaders = options.ExtraHTTPHeaders,
            Offline = options.Offline,
            HttpCredentials = options.HttpCredentials,
            ColorScheme = options.ColorScheme,
            ReducedMotion = options.ReducedMotion,
            ForcedColors = options.ForcedColors,
            Contrast = options.Contrast,
            RecordHarPath = options.RecordHarPath,
            RecordHarContent = options.RecordHarContent,
            RecordHarMode = options.RecordHarMode,
            RecordHarOmitContent = options.RecordHarOmitContent,
            RecordHarUrlFilter = options.RecordHarUrlFilter,
            RecordHarUrlFilterString = options.RecordHarUrlFilterString,
            RecordHarUrlFilterRegex = options.RecordHarUrlFilterRegex,
            RecordVideoDir = options.RecordVideoDir,
            RecordVideoSize = options.RecordVideoSize,
            Proxy = options.Proxy,
            StorageState = options.StorageState,
            StorageStatePath = options.StorageStatePath,
            ServiceWorkers = options.ServiceWorkers,
            BaseURL = options.BaseURL,
            StrictSelectors = options.StrictSelectors,
            ClientCertificates = options.ClientCertificates,
        };

        return await WrapApiCallAsync(
            async () =>
        {
            var context = (BrowserContext)await NewContextAsync(contextOptions).ConfigureAwait(false);
            var page = (Page)await context.NewPageAsync().ConfigureAwait(false);
            page.OwnedContext = context;
            context.OwnerPage = page;
            return page;
        },
            false,
            "Create page").ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ValueTask DisposeAsync() => new ValueTask(CloseAsync());

    internal static Dictionary<string, object>? GetVideoArgs(string? recordVideoDir, RecordVideoSize? recordVideoSize)
    {
        Dictionary<string, object>? recordVideoArgs = null;

        if (recordVideoSize != null && string.IsNullOrEmpty(recordVideoDir))
        {
            throw new PlaywrightException("\"RecordVideoSize\" option requires \"RecordVideoDir\" to be specified");
        }

        if (!string.IsNullOrEmpty(recordVideoDir))
        {
            recordVideoArgs = new()
            {
                ["dir"] = System.IO.Path.Combine(Environment.CurrentDirectory, recordVideoDir),
            };

            if (recordVideoSize != null)
            {
                recordVideoArgs["size"] = recordVideoSize;
            }
        }

        return recordVideoArgs;
    }

    internal void ConnectToBrowserType(BrowserType browserType, string? tracesDir)
    {
        // Note: when using connect(), `browserType` is different from `this.parent`.
        // This is why browser type is not wired up in the constructor, and instead this separate method is called later on.
        _browserType = browserType;
        _tracesDir = tracesDir;
        foreach (var context in _contexts)
        {
            context._tracing._tracesDir = this._tracesDir;
            browserType.Playwright._selectors._contextsForSelectors.Add(context);
        }
    }

    private void DidCreateContext(BrowserContext context)
    {
        context._browser = this;
        _contexts.Add(context);
        // Note: when connecting to a browser, initial contexts arrive before `_browserType` is set,
        // and will be configured later in `ConnectToBrowserType`.
        if (_browserType != null)
        {
            context._tracing._tracesDir = _tracesDir;
            _browserType.Playwright._selectors._contextsForSelectors.Add(context);
        }
    }

    internal void DidClose()
    {
        IsConnected = false;
        Disconnected?.Invoke(this, this);
        _closedTcs.TrySetResult(true);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<ICDPSession> NewBrowserCDPSessionAsync()
        => await SendMessageToServerAsync<CDPSession>(
        "newBrowserCDPSession").ConfigureAwait(false);

    internal static Dictionary<string, string?>[]? ToClientCertificatesProtocol(IEnumerable<ClientCertificate>? clientCertificates)
    {
        if (clientCertificates == null)
        {
            return null;
        }
        return clientCertificates.Select(clientCertificate => new Dictionary<string, string?>
        {
            ["origin"] = clientCertificate.Origin,
            ["passphrase"] = clientCertificate.Passphrase,
            ["cert"] = ReadClientCertificateFile(clientCertificate.CertPath, clientCertificate.Cert),
            ["key"] = ReadClientCertificateFile(clientCertificate.KeyPath, clientCertificate.Key),
            ["pfx"] = ReadClientCertificateFile(clientCertificate.PfxPath, clientCertificate.Pfx),
        }
                .Where(kv => kv.Value != null)
                .ToDictionary(kv => kv.Key, kv => kv.Value))
            .ToArray();
    }

    private static string? ReadClientCertificateFile(string? path, byte[]? value)
    {
        if (value != null)
        {
            return Convert.ToBase64String(value);
        }
        if (path != null)
        {
            return Convert.ToBase64String(File.ReadAllBytes(path));
        }
        return null;
    }
}
