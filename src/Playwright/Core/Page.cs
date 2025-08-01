/*
 * MIT License
 *
 * Copyright (c) 2020 Darío Kondratiuk
 * Copyright (c) 2020 Meir Blachman
 * Modifications copyright (c) Microsoft Corporation.
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright.Helpers;
using Microsoft.Playwright.Transport;
using Microsoft.Playwright.Transport.Protocol;

namespace Microsoft.Playwright.Core;

internal class Page : ChannelOwner, IPage
{
    private readonly List<Frame> _frames = new();
    private readonly PageInitializer _initializer;

    internal readonly List<Worker> _workers = new();
    internal readonly TimeoutSettings _timeoutSettings;
    private readonly List<HarRouter> _harRouters = new();
    private readonly Dictionary<int, LocatorHandler> _locatorHandlers = new();
    private readonly List<WebSocketRouteHandler> _webSocketRoutes = new();
    private List<RouteHandler> _routes = new();
    private Video? _video;
    private string? _closeReason;

    internal Page(ChannelOwner parent, string guid, PageInitializer initializer) : base(parent, guid)
    {
        Context = (BrowserContext)parent;
        _timeoutSettings = new(Context._timeoutSettings);

        MainFrame = initializer.MainFrame;
        MainFrame.Page = this;
        _frames.Add(MainFrame);
        if (initializer.ViewportSize != null)
        {
            ViewportSize = new() { Width = initializer.ViewportSize.Width, Height = initializer.ViewportSize.Height };
        }

        IsClosed = initializer.IsClosed;
        Accessibility = new Accessibility(this);
        Keyboard = new Keyboard(this);
        Touchscreen = new Touchscreen(this);
        Mouse = new Mouse(this);
        APIRequest = Context._request;

        _initializer = initializer;

        Close += (_, _) => ClosedOrCrashedTcs.TrySetResult(true);
        Crash += (_, _) => ClosedOrCrashedTcs.TrySetResult(true);
    }

    private event EventHandler<IRequest>? _requestImpl;

    private event EventHandler<IResponse>? _responseImpl;

    private event EventHandler<IRequest>? _requestFinishedImpl;

    private event EventHandler<IRequest>? _requestFailedImpl;

    private event EventHandler<IFileChooser>? _fileChooserImpl;

    private event EventHandler<IConsoleMessage>? _consoleImpl;

    private event EventHandler<IDialog>? _dialogImpl;

    public event EventHandler<IConsoleMessage>? Console
    {
        add => this._consoleImpl = UpdateEventHandler("console", this._consoleImpl, value, true);
        remove => this._consoleImpl = UpdateEventHandler("console", this._consoleImpl, value, false);
    }

    public event EventHandler<IPage>? Popup;

    public event EventHandler<IRequest>? Request
    {
        add => this._requestImpl = UpdateEventHandler("request", this._requestImpl, value, true);
        remove => this._requestImpl = UpdateEventHandler("request", this._requestImpl, value, false);
    }

    public event EventHandler<IWebSocket>? WebSocket;

    public event EventHandler<IResponse>? Response
    {
        add => this._responseImpl = UpdateEventHandler("response", this._responseImpl, value, true);
        remove => this._responseImpl = UpdateEventHandler("response", this._responseImpl, value, false);
    }

    public event EventHandler<IRequest>? RequestFinished
    {
        add => this._requestFinishedImpl = UpdateEventHandler("requestFinished", this._requestFinishedImpl, value, true);
        remove => this._requestFinishedImpl = UpdateEventHandler("requestFinished", this._requestFinishedImpl, value, false);
    }

    public event EventHandler<IRequest>? RequestFailed
    {
        add => this._requestFailedImpl = UpdateEventHandler("requestFailed", this._requestFailedImpl, value, true);
        remove => this._requestFailedImpl = UpdateEventHandler("requestFailed", this._requestFailedImpl, value, false);
    }

    public event EventHandler<IDialog>? Dialog
    {
        add => this._dialogImpl = UpdateEventHandler("dialog", this._dialogImpl, value, true);
        remove => this._dialogImpl = UpdateEventHandler("dialog", this._dialogImpl, value, false);
    }

    public event EventHandler<IFrame>? FrameAttached;

    public event EventHandler<IFrame>? FrameDetached;

    public event EventHandler<IFrame>? FrameNavigated;

    public event EventHandler<IFileChooser>? FileChooser
    {
        add => this._fileChooserImpl = UpdateEventHandler("fileChooser", this._fileChooserImpl, value, true);
        remove => this._fileChooserImpl = UpdateEventHandler("fileChooser", this._fileChooserImpl, value, false);
    }

    public event EventHandler<IPage>? Load;

    public event EventHandler<IPage>? DOMContentLoaded;

    public event EventHandler<IPage>? Close;

    public event EventHandler<IPage>? Crash;

    public event EventHandler<string>? PageError;

    public event EventHandler<IWorker>? Worker;

    public event EventHandler<IDownload>? Download;

    public bool IsClosed { get; private set; }

    internal bool CloseWasCalled { get; private set; }

    IFrame IPage.MainFrame => MainFrame;

    public Frame MainFrame { get; }

    IBrowserContext IPage.Context => Context;

    public BrowserContext Context { get; set; }

    public PageViewportSizeResult? ViewportSize { get; private set; }

    public IAccessibility Accessibility
    {
        get;
    }

    public IMouse Mouse
    {
        get;
    }

    public IClock Clock => Context.Clock;

    public string Url => MainFrame.Url;

    public IReadOnlyList<IFrame> Frames => _frames.ToList().AsReadOnly();

    public IKeyboard Keyboard { get; }

    public ITouchscreen Touchscreen { get; }

    public IReadOnlyList<IWorker> Workers => _workers;

    public IVideo? Video
    {
        get
        {
            if (Context.VideosDir() == null)
            {
                return null;
            }

            return ForceVideo();
        }
        set => _video = value as Video;
    }

    internal BrowserContext? OwnedContext { get; set; }

    internal Dictionary<string, Delegate> Bindings { get; } = new();

    internal Page Opener => _initializer.Opener;

    internal TaskCompletionSource<bool> ClosedOrCrashedTcs { get; } = new();

    public IAPIRequestContext APIRequest { get; }

    internal override void OnMessage(string method, JsonElement serverParams)
    {
        switch (method)
        {
            case "close":
                OnClose();
                break;
            case "crash":
                Channel_Crashed();
                break;
            case "bindingCall":
                Channel_BindingCall(
                    this,
                    serverParams.GetProperty("binding").ToObject<BindingCall>(_connection.DefaultJsonSerializerOptions));
                break;
            case "route":
                var route = serverParams.GetProperty("route").ToObject<Route>(_connection.DefaultJsonSerializerOptions);
                Channel_Route(this, route);
                break;
            case "webSocketRoute":
                var webSocketRoute = serverParams.GetProperty("webSocketRoute").ToObject<WebSocketRoute>(_connection.DefaultJsonSerializerOptions);
                _ = OnWebSocketRouteAsync(webSocketRoute).ConfigureAwait(false);
                break;
            case "popup":
                Popup?.Invoke(this, serverParams.GetProperty("page").ToObject<Page>(_connection.DefaultJsonSerializerOptions));
                break;
            case "fileChooser":
                _fileChooserImpl?.Invoke(this, new FileChooser(this, serverParams.GetProperty("element").ToObject<ElementHandle>(_connection.DefaultJsonSerializerOptions), serverParams.GetProperty("isMultiple").ToObject<bool>(_connection.DefaultJsonSerializerOptions)));
                break;
            case "frameAttached":
                Channel_FrameAttached(this, serverParams.GetProperty("frame").ToObject<Frame>(_connection.DefaultJsonSerializerOptions));
                break;
            case "frameDetached":
                Channel_FrameDetached(this, serverParams.GetProperty("frame").ToObject<Frame>(_connection.DefaultJsonSerializerOptions));
                break;
            case "locatorHandlerTriggered":
                _ = Channel_LocatorHandlerTriggeredAsync(serverParams.GetProperty("uid").GetInt32());
                break;
            case "webSocket":
                WebSocket?.Invoke(this, serverParams.GetProperty("webSocket").ToObject<WebSocket>(_connection.DefaultJsonSerializerOptions));
                break;
            case "download":
                Download?.Invoke(this, new Download(this, serverParams.GetProperty("url").ToObject<string>(_connection.DefaultJsonSerializerOptions), serverParams.GetProperty("suggestedFilename").ToObject<string>(_connection.DefaultJsonSerializerOptions), serverParams.GetProperty("artifact").ToObject<Artifact>(_connection.DefaultJsonSerializerOptions)));
                break;
            case "video":
                ForceVideo().ArtifactReady(serverParams.GetProperty("artifact").ToObject<Artifact>(_connection.DefaultJsonSerializerOptions));
                break;
            case "viewportSizeChanged":
                var size = serverParams.GetProperty("viewportSize").ToObject<ViewportSize>(_connection.DefaultJsonSerializerOptions);
                ViewportSize = new() { Width = size.Width, Height = size.Height };
                break;
            case "worker":
                var worker = serverParams.GetProperty("worker").ToObject<Worker>(_connection.DefaultJsonSerializerOptions);
                _workers.Add(worker);
                worker.Page = this;
                Worker?.Invoke(this, worker);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IFrame Frame(string name)
        => Frames.FirstOrDefault(f => f.Name == name);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IFrame FrameByUrl(string urlString) => Frames.FirstOrDefault(f => Context.UrlMatches(f.Url, urlString));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IFrame FrameByUrl(Regex urlRegex) => Frames.FirstOrDefault(f => urlRegex.IsMatch(f.Url));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public IFrame FrameByUrl(Func<string, bool> urlFunc) => Frames.FirstOrDefault(f => urlFunc(f.Url));

    IFrameLocator IPage.FrameLocator(string selector) => MainFrame.FrameLocator(selector);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<string> TitleAsync() => MainFrame.TitleAsync();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task BringToFrontAsync() => SendMessageToServerAsync("bringToFront");

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IPage?> OpenerAsync() => Task.FromResult<IPage?>(Opener?.IsClosed == false ? Opener : null);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RequestGCAsync() => SendMessageToServerAsync("requestGC");

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task EmulateMediaAsync(PageEmulateMediaOptions? options = default)
    {
        var args = new Dictionary<string, object?>
        {
            ["media"] = options?.Media == Media.Null ? "no-override" : options?.Media,
            ["colorScheme"] = options?.ColorScheme == ColorScheme.Null ? "no-override" : options?.ColorScheme,
            ["reducedMotion"] = options?.ReducedMotion == ReducedMotion.Null ? "no-override" : options?.ReducedMotion,
            ["forcedColors"] = options?.ForcedColors == ForcedColors.Null ? "no-override" : options?.ForcedColors,
            ["contrast"] = options?.Contrast == Contrast.Null ? "no-override" : options?.Contrast,
        };
        return SendMessageToServerAsync("emulateMedia", args);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse?> GotoAsync(string url, PageGotoOptions? options = default)
        => MainFrame.GotoAsync(url, new() { WaitUntil = options?.WaitUntil, Timeout = options?.Timeout, Referer = options?.Referer });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task WaitForURLAsync(string url, PageWaitForURLOptions? options = default)
        => MainFrame.WaitForURLAsync(url, new() { WaitUntil = options?.WaitUntil, Timeout = options?.Timeout });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task WaitForURLAsync(Regex url, PageWaitForURLOptions? options = default)
        => MainFrame.WaitForURLAsync(url, new() { WaitUntil = options?.WaitUntil, Timeout = options?.Timeout });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task WaitForURLAsync(Func<string, bool> url, PageWaitForURLOptions? options = default)
        => MainFrame.WaitForURLAsync(url, new() { WaitUntil = options?.WaitUntil, Timeout = options?.Timeout });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IConsoleMessage> WaitForConsoleMessageAsync(PageWaitForConsoleMessageOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Console, null, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IFileChooser> WaitForFileChooserAsync(PageWaitForFileChooserOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.FileChooser, null, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IPage> WaitForPopupAsync(PageWaitForPopupOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Popup, null, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IWebSocket> WaitForWebSocketAsync(PageWaitForWebSocketOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.WebSocket, null, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IWorker> WaitForWorkerAsync(PageWaitForWorkerOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Worker, null, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse?> WaitForNavigationAsync(PageWaitForNavigationOptions? options = default)
        => MainFrame.WaitForNavigationAsync(new()
        {
            Url = options?.Url,
            UrlString = options?.UrlString,
            UrlRegex = options?.UrlRegex,
            UrlFunc = options?.UrlFunc,
            WaitUntil = options?.WaitUntil,
            Timeout = options?.Timeout,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse?> RunAndWaitForNavigationAsync(Func<Task> action, PageRunAndWaitForNavigationOptions? options = default)
        => MainFrame.RunAndWaitForNavigationAsync(action, new()
        {
            Url = options?.Url,
            UrlString = options?.UrlString,
            UrlRegex = options?.UrlRegex,
            UrlFunc = options?.UrlFunc,
            WaitUntil = options?.WaitUntil,
            Timeout = options?.Timeout,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> WaitForRequestAsync(string urlOrPredicate, PageWaitForRequestOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Request, null, e => Context.UrlMatches(e.Url, urlOrPredicate), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> WaitForRequestAsync(Regex urlOrPredicate, PageWaitForRequestOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Request, null, e => urlOrPredicate.IsMatch(e.Url), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> WaitForRequestAsync(Func<IRequest, bool> urlOrPredicate, PageWaitForRequestOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Request, null, e => urlOrPredicate(e), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> WaitForRequestFinishedAsync(PageWaitForRequestFinishedOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.RequestFinished, null, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse> WaitForResponseAsync(string urlOrPredicate, PageWaitForResponseOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Response, null, e => Context.UrlMatches(e.Url, urlOrPredicate), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse> WaitForResponseAsync(Regex urlOrPredicate, PageWaitForResponseOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Response, null, e => urlOrPredicate.IsMatch(e.Url), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse> WaitForResponseAsync(Func<IResponse, bool> urlOrPredicate, PageWaitForResponseOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Response, null, e => urlOrPredicate(e), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IConsoleMessage> RunAndWaitForConsoleMessageAsync(Func<Task> action, PageRunAndWaitForConsoleMessageOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Console, action, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IDownload> WaitForDownloadAsync(PageWaitForDownloadOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Download, null, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IDownload> RunAndWaitForDownloadAsync(Func<Task> action, PageRunAndWaitForDownloadOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Download, action, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IFileChooser> RunAndWaitForFileChooserAsync(Func<Task> action, PageRunAndWaitForFileChooserOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.FileChooser, action, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IPage> RunAndWaitForPopupAsync(Func<Task> action, PageRunAndWaitForPopupOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Popup, action, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> RunAndWaitForRequestFinishedAsync(Func<Task> action, PageRunAndWaitForRequestFinishedOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.RequestFinished, action, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IWebSocket> RunAndWaitForWebSocketAsync(Func<Task> action, PageRunAndWaitForWebSocketOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.WebSocket, action, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IWorker> RunAndWaitForWorkerAsync(Func<Task> action, PageRunAndWaitForWorkerOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Worker, action, options?.Predicate, options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> RunAndWaitForRequestAsync(Func<Task> action, string urlOrPredicate, PageRunAndWaitForRequestOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Request, action, e => Context.UrlMatches(e.Url, urlOrPredicate), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> RunAndWaitForRequestAsync(Func<Task> action, Regex urlOrPredicate, PageRunAndWaitForRequestOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Request, action, e => urlOrPredicate.IsMatch(e.Url), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IRequest> RunAndWaitForRequestAsync(Func<Task> action, Func<IRequest, bool> urlOrPredicate, PageRunAndWaitForRequestOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Request, action, e => urlOrPredicate(e), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse> RunAndWaitForResponseAsync(Func<Task> action, string urlOrPredicate, PageRunAndWaitForResponseOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Response, action, e => Context.UrlMatches(e.Url, urlOrPredicate), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse> RunAndWaitForResponseAsync(Func<Task> action, Regex urlOrPredicate, PageRunAndWaitForResponseOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Response, action, e => urlOrPredicate.IsMatch(e.Url), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IResponse> RunAndWaitForResponseAsync(Func<Task> action, Func<IResponse, bool> urlOrPredicate, PageRunAndWaitForResponseOptions? options = default)
        => InnerWaitForEventAsync(PageEvent.Response, action, e => urlOrPredicate(e), options?.Timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IJSHandle> WaitForFunctionAsync(string expression, object? arg = default, PageWaitForFunctionOptions? options = default)
        => MainFrame.WaitForFunctionAsync(expression, arg, new() { PollingInterval = options?.PollingInterval, Timeout = options?.Timeout });

    internal TargetClosedException _closeErrorWithReason()
    {
        return new TargetClosedException(_closeReason ?? Context._effectiveCloseReason());
    }

    internal async Task<T> InnerWaitForEventAsync<T>(PlaywrightEvent<T> pageEvent, Func<Task>? action = default, Func<T, bool>? predicate = default, float? timeout = default)
    {
        if (pageEvent == null)
        {
            throw new ArgumentException("Page event is required", nameof(pageEvent));
        }

        timeout = _timeoutSettings.Timeout(timeout);
        using var waiter = new Waiter(this, $"page.WaitForEventAsync(\"{typeof(T)}\")");
        waiter.RejectOnTimeout(Convert.ToInt32(timeout, CultureInfo.InvariantCulture), $"Timeout {timeout}ms exceeded while waiting for event \"{pageEvent.Name}\"");

        if (pageEvent.Name != PageEvent.Crash.Name)
        {
            waiter.RejectOnEvent<IPage>(this, PageEvent.Crash.Name, new PlaywrightException("Page crashed"));
        }

        if (pageEvent.Name != PageEvent.Close.Name)
        {
            waiter.RejectOnEvent<IPage>(this, PageEvent.Close.Name, () => _closeErrorWithReason());
        }

        var waitForEventTask = waiter.WaitForEventAsync(this, pageEvent.Name, predicate);
        if (action != null)
        {
            await waiter.CancelWaitOnExceptionAsync(waitForEventTask, action).ConfigureAwait(false);
        }

        return await waitForEventTask.ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task CloseAsync(PageCloseOptions? options = default)
    {
        _closeReason = options?.Reason;
        CloseWasCalled = true;
        try
        {
            await SendMessageToServerAsync(
            "close",
            new Dictionary<string, object?>
            {
                ["runBeforeUnload"] = options?.RunBeforeUnload ?? false,
            }).ConfigureAwait(false);
            if (OwnedContext != null)
            {
                await OwnedContext.CloseAsync().ConfigureAwait(false);
            }
        }
        catch (Exception e) when (DriverMessages.IsTargetClosedError(e) && options?.RunBeforeUnload != true)
        {
            // Swallow exception
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<T> EvaluateAsync<T>(string expression, object? arg) => MainFrame.EvaluateAsync<T>(expression, arg);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<JsonElement?> EvalOnSelectorAsync(string selector, string expression, object? arg) => MainFrame.EvalOnSelectorAsync(selector, expression, arg);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<T> EvalOnSelectorAsync<T>(string selector, string expression, object? arg = null, PageEvalOnSelectorOptions? options = null)
        => MainFrame.EvalOnSelectorAsync<T>(selector, expression, arg, new() { Strict = options?.Strict });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator Locator(string selector, PageLocatorOptions? options = default)
        => MainFrame.Locator(selector, new()
        {
            Has = options?.Has,
            HasText = options?.HasText,
            HasTextString = options?.HasTextString,
            HasTextRegex = options?.HasTextRegex,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IElementHandle?> QuerySelectorAsync(string selector, PageQuerySelectorOptions? options = null)
        => MainFrame.QuerySelectorAsync(selector, new() { Strict = options?.Strict });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<T> EvalOnSelectorAsync<T>(string selector, string expression, object? arg) => MainFrame.EvalOnSelectorAsync<T>(selector, expression, arg);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<JsonElement?> EvalOnSelectorAllAsync(string selector, string expression, object? arg) => MainFrame.EvalOnSelectorAllAsync(selector, expression, arg);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<T> EvalOnSelectorAllAsync<T>(string selector, string expression, object? arg) => MainFrame.EvalOnSelectorAllAsync<T>(selector, expression, arg);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task FillAsync(string selector, string value, PageFillOptions? options = default)
#pragma warning disable CS0612 // Type or member is obsolete
        => MainFrame.FillAsync(selector, value, new() { NoWaitAfter = options?.NoWaitAfter, Timeout = options?.Timeout, Force = options?.Force, Strict = options?.Strict });
#pragma warning restore CS0612 // Type or member is obsolete

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetInputFilesAsync(string selector, string files, PageSetInputFilesOptions? options = default)
        => MainFrame.SetInputFilesAsync(selector, files, Map(options));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetInputFilesAsync(string selector, IEnumerable<string> files, PageSetInputFilesOptions? options = default)
        => MainFrame.SetInputFilesAsync(selector, files, Map(options));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetInputFilesAsync(string selector, FilePayload files, PageSetInputFilesOptions? options = default)
        => MainFrame.SetInputFilesAsync(selector, files, Map(options));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetInputFilesAsync(string selector, IEnumerable<FilePayload> files, PageSetInputFilesOptions? options = default)
        => MainFrame.SetInputFilesAsync(selector, files, Map(options));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task TypeAsync(string selector, string text, PageTypeOptions? options = default)
        => MainFrame.TypeAsync(selector, text, new()
        {
            Delay = options?.Delay,
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task FocusAsync(string selector, PageFocusOptions? options = default)
        => MainFrame.FocusAsync(selector, new()
        {
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task HoverAsync(string selector, PageHoverOptions? options = default)
        => MainFrame.HoverAsync(
            selector,
            new()
            {
                Position = options?.Position,
                Modifiers = options?.Modifiers,
                Force = options?.Force,
                Timeout = options?.Timeout,
                Trial = options?.Trial,
                Strict = options?.Strict,
            });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task PressAsync(string selector, string key, PagePressOptions? options = default)
        => MainFrame.PressAsync(selector, key, new()
        {
            Delay = options?.Delay,
#pragma warning disable CS0612 // Type or member is obsolete
            NoWaitAfter = options?.NoWaitAfter,
#pragma warning restore CS0612 // Type or member is obsolete
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, string values, PageSelectOptionOptions? options = default)
        => SelectOptionAsync(selector, new[] { values }, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<string> values, PageSelectOptionOptions? options = default)
        => SelectOptionAsync(selector, values.Select(x => new SelectOptionValueProtocol() { ValueOrLabel = x }), options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IElementHandle values, PageSelectOptionOptions? options = default)
        => SelectOptionAsync(selector, new[] { values }, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<IElementHandle> values, PageSelectOptionOptions? options = default)
        => MainFrame.SelectOptionAsync(selector, values, new()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            NoWaitAfter = options?.NoWaitAfter,
#pragma warning restore CS0612 // Type or member is obsolete
            Timeout = options?.Timeout,
            Force = options?.Force,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, SelectOptionValue values, PageSelectOptionOptions? options = default)
        => SelectOptionAsync(selector, new[] { values }, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<SelectOptionValue> values, PageSelectOptionOptions? options = default)
        => SelectOptionAsync(selector, values.Select(x => SelectOptionValueProtocol.From(x)), options);

    internal Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<SelectOptionValueProtocol> values, PageSelectOptionOptions? options = default)
        => MainFrame.SelectOptionAsync(selector, values, new()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            NoWaitAfter = options?.NoWaitAfter,
#pragma warning restore CS0612 // Type or member is obsolete
            Timeout = options?.Timeout,
            Force = options?.Force,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task WaitForTimeoutAsync(float timeout) => MainFrame.WaitForTimeoutAsync(timeout);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IElementHandle?> WaitForSelectorAsync(string selector, PageWaitForSelectorOptions? options = default)
        => MainFrame.WaitForSelectorAsync(selector, new()
        {
            State = options?.State,
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<JsonElement?> EvaluateAsync(string expression, object? arg) => MainFrame.EvaluateAsync(expression, arg);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<byte[]> ScreenshotAsync(PageScreenshotOptions? options = default)
    {
        options ??= new PageScreenshotOptions();
        if (options.Type == null && !string.IsNullOrEmpty(options.Path) && options.Path != null)
        {
            options.Type = ElementHandle.DetermineScreenshotType(options.Path);
        }

        var result = (await SendMessageToServerAsync("screenshot", new Dictionary<string, object?>
        {
            ["fullPage"] = options.FullPage,
            ["omitBackground"] = options.OmitBackground,
            ["clip"] = options.Clip,
            ["path"] = options.Path,
            ["type"] = options.Type,
            ["timeout"] = _timeoutSettings.Timeout(options.Timeout),
            ["animations"] = options.Animations,
            ["caret"] = options.Caret,
            ["scale"] = options.Scale,
            ["quality"] = options.Quality,
            ["maskColor"] = options.MaskColor,
            ["style"] = options.Style,
            ["mask"] = options.Mask?.Select(locator => new Dictionary<string, object>
            {
                ["frame"] = ((Locator)locator)._frame,
                ["selector"] = ((Locator)locator)._selector,
            }).ToArray(),
        }).ConfigureAwait(false)).Value.GetProperty("binary").GetBytesFromBase64();

        if (!string.IsNullOrEmpty(options.Path))
        {
            Directory.CreateDirectory(new FileInfo(options.Path).Directory.FullName);
            File.WriteAllBytes(options.Path, result);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetContentAsync(string html, PageSetContentOptions? options = default)
        => MainFrame.SetContentAsync(html, new() { WaitUntil = options?.WaitUntil, Timeout = options?.Timeout });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<string> ContentAsync() => MainFrame.ContentAsync();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetExtraHTTPHeadersAsync(IEnumerable<KeyValuePair<string, string>> headers)
        => SendMessageToServerAsync(
            "setExtraHTTPHeaders",
            new Dictionary<string, object?>
            {
                ["headers"] = headers.Select(kv => new HeaderEntry { Name = kv.Key, Value = kv.Value }),
            });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IElementHandle> QuerySelectorAsync(string selector) => MainFrame.QuerySelectorAsync(selector);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IReadOnlyList<IElementHandle>> QuerySelectorAllAsync(string selector)
        => MainFrame.QuerySelectorAllAsync(selector);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IJSHandle> EvaluateHandleAsync(string expression, object? arg) => MainFrame.EvaluateHandleAsync(expression, arg);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IElementHandle> AddScriptTagAsync(PageAddScriptTagOptions? options = default)
        => MainFrame.AddScriptTagAsync(new()
        {
            Url = options?.Url,
            Path = options?.Path,
            Content = options?.Content,
            Type = options?.Type,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<IElementHandle> AddStyleTagAsync(PageAddStyleTagOptions? options = default)
        => MainFrame.AddStyleTagAsync(new()
        {
            Url = options?.Url,
            Path = options?.Path,
            Content = options?.Content,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ClickAsync(string selector, PageClickOptions? options = default)
        => MainFrame.ClickAsync(
            selector,
            new()
            {
                Button = options?.Button,
                ClickCount = options?.ClickCount,
                Delay = options?.Delay,
                Position = options?.Position,
                Modifiers = options?.Modifiers,
                Force = options?.Force,
#pragma warning disable CS0612 // Type or member is obsolete
                NoWaitAfter = options?.NoWaitAfter,
#pragma warning restore CS0612 // Type or member is obsolete
                Timeout = options?.Timeout,
                Trial = options?.Trial,
                Strict = options?.Strict,
            });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task DblClickAsync(string selector, PageDblClickOptions? options = default)
        => MainFrame.DblClickAsync(selector, new()
        {
            Delay = options?.Delay,
            Button = options?.Button,
            Position = options?.Position,
            Modifiers = options?.Modifiers,
            Timeout = options?.Timeout,
            Force = options?.Force,
            Trial = options?.Trial,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<IResponse?> GoBackAsync(PageGoBackOptions? options = default)
        => await SendMessageToServerAsync<Response>("goBack", new Dictionary<string, object?>
        {
            ["timeout"] = _timeoutSettings.NavigationTimeout(options?.Timeout),
            ["waitUntil"] = options?.WaitUntil,
        }).ConfigureAwait(false);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<IResponse?> GoForwardAsync(PageGoForwardOptions? options = default)
        => await SendMessageToServerAsync<Response>("goForward", new Dictionary<string, object?>
        {
            ["timeout"] = _timeoutSettings.NavigationTimeout(options?.Timeout),
            ["waitUntil"] = options?.WaitUntil,
        }).ConfigureAwait(false);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<IResponse?> ReloadAsync(PageReloadOptions? options = default)
        => await SendMessageToServerAsync<Response>("reload", new Dictionary<string, object?>
        {
            ["timeout"] = _timeoutSettings.NavigationTimeout(options?.Timeout),
            ["waitUntil"] = options?.WaitUntil,
        }).ConfigureAwait(false);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync(string name, Action callback, PageExposeBindingOptions? options = default)
#pragma warning disable CS0612 // Type or member is obsolete
        => InnerExposeBindingAsync(name, callback, options?.Handle ?? false);
#pragma warning restore CS0612 // Type or member is obsolete

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync(string name, Action<BindingSource> callback)
        => InnerExposeBindingAsync(name, callback);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync<T>(string name, Action<BindingSource, T> callback)
        => InnerExposeBindingAsync(name, callback);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync<TResult>(string name, Func<BindingSource, TResult> callback)
        => InnerExposeBindingAsync(name, callback);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync<TResult>(string name, Func<BindingSource, IJSHandle, TResult> callback)
        => InnerExposeBindingAsync(name, callback, true);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync<T, TResult>(string name, Func<BindingSource, T, TResult> callback)
        => InnerExposeBindingAsync(name, callback);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync<T1, T2, TResult>(string name, Func<BindingSource, T1, T2, TResult> callback)
        => InnerExposeBindingAsync(name, callback);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync<T1, T2, T3, TResult>(string name, Func<BindingSource, T1, T2, T3, TResult> callback)
        => InnerExposeBindingAsync(name, callback);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeBindingAsync<T1, T2, T3, T4, TResult>(string name, Func<BindingSource, T1, T2, T3, T4, TResult> callback)
        => InnerExposeBindingAsync(name, callback);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeFunctionAsync(string name, Action callback)
        => ExposeBindingAsync(name, (BindingSource _) => callback());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeFunctionAsync<T>(string name, Action<T> callback)
        => ExposeBindingAsync(name, (BindingSource _, T t) => callback(t));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeFunctionAsync<TResult>(string name, Func<TResult> callback)
        => ExposeBindingAsync(name, (BindingSource _) => callback());

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeFunctionAsync<T, TResult>(string name, Func<T, TResult> callback)
        => ExposeBindingAsync(name, (BindingSource _, T t) => callback(t));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeFunctionAsync<T1, T2, TResult>(string name, Func<T1, T2, TResult> callback)
        => ExposeBindingAsync(name, (BindingSource _, T1 t1, T2 t2) => callback(t1, t2));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeFunctionAsync<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> callback)
        => ExposeBindingAsync(name, (BindingSource _, T1 t1, T2 t2, T3 t3) => callback(t1, t2, t3));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task ExposeFunctionAsync<T1, T2, T3, T4, TResult>(string name, Func<T1, T2, T3, T4, TResult> callback)
        => ExposeBindingAsync(name, (BindingSource _, T1 t1, T2 t2, T3 t3, T4 t4) => callback(t1, t2, t3, t4));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task<byte[]> PdfAsync(PagePdfOptions? options = default)
    {
        options ??= new();
        byte[] result = (await SendMessageToServerAsync("pdf", new Dictionary<string, object?>
        {
            ["scale"] = options?.Scale,
            ["displayHeaderFooter"] = options?.DisplayHeaderFooter,
            ["printBackground"] = options?.PrintBackground,
            ["landscape"] = options?.Landscape,
            ["preferCSSPageSize"] = options?.PreferCSSPageSize,
            ["pageRanges"] = options?.PageRanges,
            ["headerTemplate"] = options?.HeaderTemplate,
            ["footerTemplate"] = options?.FooterTemplate,
            ["margin"] = options?.Margin,
            ["width"] = options?.Width,
            ["format"] = options?.Format,
            ["height"] = options?.Height,
            ["outline"] = options?.Outline,
            ["tagged"] = options?.Tagged,
        }).ConfigureAwait(false)).Value.GetProperty("pdf").GetBytesFromBase64();

        if (!string.IsNullOrEmpty(options?.Path))
        {
            Directory.CreateDirectory(new FileInfo(options?.Path).Directory.FullName);
            File.WriteAllBytes(options?.Path, result);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task AddInitScriptAsync(string? script, string? scriptPath)
        => SendMessageToServerAsync(
            "addInitScript",
            new Dictionary<string, object?>
            {
                ["source"] = ScriptsHelper.EvaluationScript(script, scriptPath, true),
            });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteAsync(string globMatch, Func<IRoute, Task> handler, PageRouteOptions? options = null)
        => RouteAsync(globMatch, null, null, handler, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteAsync(string globMatch, Action<IRoute> handler, PageRouteOptions? options = null)
        => RouteAsync(globMatch, null, null, handler, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteAsync(Regex reMatch, Action<IRoute> handler, PageRouteOptions? options = null)
         => RouteAsync(null, reMatch, null, handler, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteAsync(Regex reMatch, Func<IRoute, Task> handler, PageRouteOptions? options = null)
         => RouteAsync(null, reMatch, null, handler, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteAsync(Func<string, bool> funcMatch, Action<IRoute> handler, PageRouteOptions? options = null)
        => RouteAsync(null, null, funcMatch, handler, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteAsync(Func<string, bool> funcMatch, Func<IRoute, Task> handler, PageRouteOptions? options = null)
        => RouteAsync(null, null, funcMatch, handler, options);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task UnrouteAllAsync(PageUnrouteAllOptions? options = default)
    {
        await UnrouteInternalAsync(_routes, [], options?.Behavior).ConfigureAwait(false);
        DisposeHarRouters();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UnrouteAsync(string globMatch, Action<IRoute>? handler)
        => UnrouteAsync(globMatch, null, null, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UnrouteAsync(string globMatch, Func<IRoute, Task> handler)
        => UnrouteAsync(globMatch, null, null, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UnrouteAsync(Regex reMatch, Action<IRoute>? handler)
        => UnrouteAsync(null, reMatch, null, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UnrouteAsync(Regex reMatch, Func<IRoute, Task> handler)
        => UnrouteAsync(null, reMatch, null, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UnrouteAsync(Func<string, bool> funcMatch, Action<IRoute>? handler)
        => UnrouteAsync(null, null, funcMatch, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UnrouteAsync(Func<string, bool> funcMatch, Func<IRoute, Task> handler)
        => UnrouteAsync(null, null, funcMatch, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task WaitForLoadStateAsync(LoadState? state = default, PageWaitForLoadStateOptions? options = default)
        => MainFrame.WaitForLoadStateAsync(state, new() { Timeout = options?.Timeout });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetViewportSizeAsync(int width, int height)
    {
        ViewportSize = new() { Width = width, Height = height };
        return SendMessageToServerAsync(
            "setViewportSize",
            new Dictionary<string, object?>
            {
                ["viewportSize"] = ViewportSize,
            });
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task SetCheckedAsync(string selector, bool checkedState, PageSetCheckedOptions? options = null)
        => checkedState ?
        MainFrame.CheckAsync(selector, new()
        {
            Position = options?.Position,
            Force = options?.Force,
            Strict = options?.Strict,
            Timeout = options?.Timeout,
            Trial = options?.Trial,
        })
        : MainFrame.UncheckAsync(selector, new()
        {
            Position = options?.Position,
            Force = options?.Force,
            Timeout = options?.Timeout,
            Trial = options?.Trial,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task CheckAsync(string selector, PageCheckOptions? options = default)
        => MainFrame.CheckAsync(selector, new()
        {
            Position = options?.Position,
            Force = options?.Force,
            Strict = options?.Strict,
            Timeout = options?.Timeout,
            Trial = options?.Trial,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task UncheckAsync(string selector, PageUncheckOptions? options = default)
        => MainFrame.UncheckAsync(selector, new()
        {
            Position = options?.Position,
            Force = options?.Force,
            Timeout = options?.Timeout,
            Trial = options?.Trial,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task DispatchEventAsync(string selector, string type, object? eventInit = default, PageDispatchEventOptions? options = default)
         => MainFrame.DispatchEventAsync(selector, type, eventInit, new() { Timeout = options?.Timeout, Strict = options?.Strict });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<string?> GetAttributeAsync(string selector, string name, PageGetAttributeOptions? options = default)
         => MainFrame.GetAttributeAsync(selector, name, new()
         {
             Timeout = options?.Timeout,
             Strict = options?.Strict,
         });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<string> InnerHTMLAsync(string selector, PageInnerHTMLOptions? options = default)
         => MainFrame.InnerHTMLAsync(selector, new()
         {
             Timeout = options?.Timeout,
             Strict = options?.Strict,
         });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<string> InnerTextAsync(string selector, PageInnerTextOptions? options = default)
         => MainFrame.InnerTextAsync(selector, new()
         {
             Timeout = options?.Timeout,
             Strict = options?.Strict,
         });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<string?> TextContentAsync(string selector, PageTextContentOptions? options = default)
         => MainFrame.TextContentAsync(selector, new()
         {
             Timeout = options?.Timeout,
             Strict = options?.Strict,
         });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task TapAsync(string selector, PageTapOptions? options = default)
        => MainFrame.TapAsync(
            selector,
            new()
            {
                Modifiers = options?.Modifiers,
                Position = options?.Position,
                Force = options?.Force,
                Timeout = options?.Timeout,
                Trial = options?.Trial,
                Strict = options?.Strict,
            });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<bool> IsCheckedAsync(string selector, PageIsCheckedOptions? options = default)
        => MainFrame.IsCheckedAsync(selector, new()
        {
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<bool> IsDisabledAsync(string selector, PageIsDisabledOptions? options = default)
        => MainFrame.IsDisabledAsync(selector, new()
        {
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<bool> IsEditableAsync(string selector, PageIsEditableOptions? options = default)
        => MainFrame.IsEditableAsync(selector, new()
        {
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<bool> IsEnabledAsync(string selector, PageIsEnabledOptions? options = default)
        => MainFrame.IsEnabledAsync(selector, new()
        {
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<bool> IsHiddenAsync(string selector, PageIsHiddenOptions? options = default)
        => MainFrame.IsHiddenAsync(selector, new()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Timeout = options?.Timeout,
#pragma warning restore CS0612 // Type or member is obsolete
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<bool> IsVisibleAsync(string selector, PageIsVisibleOptions? options = default)
        => MainFrame.IsVisibleAsync(selector, new()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Timeout = options?.Timeout,
#pragma warning restore CS0612 // Type or member is obsolete
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task PauseAsync()
    {
        var defaultNavigationTimeout = Context._timeoutSettings.DefaultNavigationTimeout;
        var defaultTimeout = Context._timeoutSettings.DefaultTimeout;
        Context.SetDefaultNavigationTimeout(0);
        Context.SetDefaultTimeout(0);
        try
        {
            await Task.WhenAny(Context.SendMessageToServerAsync("pause"), ClosedOrCrashedTcs.Task).ConfigureAwait(false);
        }
        finally
        {
            Context.SetDefaultNavigationTimeoutImpl(defaultNavigationTimeout);
            Context.SetDefaultTimeoutImpl(defaultTimeout);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void SetDefaultNavigationTimeout(float timeout)
    {
        _timeoutSettings.SetDefaultNavigationTimeout(timeout);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void SetDefaultTimeout(float timeout)
    {
        _timeoutSettings.SetDefaultTimeout(timeout);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task<string> InputValueAsync(string selector, PageInputValueOptions? options = null)
        => MainFrame.InputValueAsync(selector, new()
        {
            Timeout = options?.Timeout,
            Strict = options?.Strict,
        });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task DragAndDropAsync(string source, string target, PageDragAndDropOptions? options = null)
        => MainFrame.DragAndDropAsync(source, target, new()
        {
            Force = options?.Force,
            Timeout = options?.Timeout,
            Trial = options?.Trial,
            Strict = options?.Strict,
            SourcePosition = options?.SourcePosition,
            TargetPosition = options?.TargetPosition,
        });

    internal void NotifyPopup(Page page) => Popup?.Invoke(this, page);

    internal void OnFrameNavigated(Frame frame)
        => FrameNavigated?.Invoke(this, frame);

    internal void FireConsole(IConsoleMessage message) => _consoleImpl?.Invoke(this, message);

    internal void FireDialog(IDialog dialog) => _dialogImpl?.Invoke(this, dialog);

    internal bool HasDialogListenersAttached() => _dialogImpl?.GetInvocationList().Length > 0;

    internal void FireRequest(IRequest request) => _requestImpl?.Invoke(this, request);

    internal void FireRequestFailed(IRequest request) => _requestFailedImpl?.Invoke(this, request);

    internal void FireRequestFinished(IRequest request) => _requestFinishedImpl?.Invoke(this, request);

    internal void FireResponse(IResponse response) => _responseImpl?.Invoke(this, response);

    internal void FireLoad() => Load?.Invoke(this, this);

    internal void FireDOMContentLoaded() => DOMContentLoaded?.Invoke(this, this);

    internal void FirePageError(string error) => PageError?.Invoke(this, error);

    private Task RouteAsync(string? globMatch, Regex? reMatch, Func<string, bool>? funcMatch, Delegate handler, PageRouteOptions? options)
        => RouteAsync(new()
        {
            urlMatcher = new URLMatch()
            {
                glob = globMatch,
                re = reMatch,
                func = funcMatch,
                baseURL = Context.BaseURL,
            },
            Handler = handler,
            Times = options?.Times,
        });

    private Task RouteAsync(RouteHandler setting)
    {
        _routes.Insert(0, setting);
        return UpdateInterceptionAsync();
    }

    private async Task UnrouteAsync(string? globMatch, Regex? reMatch, Func<string, bool>? funcMatch, Delegate? handler)
    {
        var removed = new List<RouteHandler>();
        var remaining = new List<RouteHandler>();
        foreach (var routeHandler in _routes)
        {
            if (routeHandler.urlMatcher.Equals(globMatch, reMatch, funcMatch, Context.BaseURL, false) && (handler == null || routeHandler.Handler == handler))
            {
                removed.Add(routeHandler);
            }
            else
            {
                remaining.Add(routeHandler);
            }
        }
        await UnrouteInternalAsync(removed, remaining, UnrouteBehavior.Default).ConfigureAwait(false);
    }

    private async Task UnrouteInternalAsync(List<RouteHandler> removed, List<RouteHandler> remaining, UnrouteBehavior? behavior)
    {
        _routes = remaining;
        if (behavior != null && behavior != UnrouteBehavior.Default)
        {
            var tasks = removed.Select(routeHandler => routeHandler.StopAsync(behavior.Value));
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return;
        }
        await UpdateInterceptionAsync().ConfigureAwait(false);
    }

    private async Task UpdateInterceptionAsync()
    {
        var patterns = RouteHandler.PrepareInterceptionPatterns(_routes);
        await SendMessageToServerAsync("setNetworkInterceptionPatterns", new Dictionary<string, object?>
        {
            ["patterns"] = patterns,
        }).ConfigureAwait(false);
    }

    internal void OnClose()
    {
        IsClosed = true;
        Context._pages.Remove(this);
        Context._backgroundPages.Remove(this);
        DisposeHarRouters();
        Close?.Invoke(this, this);
    }

    private void Channel_Crashed()
    {
        Crash?.Invoke(this, this);
    }

    private void Channel_BindingCall(object sender, BindingCall bindingCall)
    {
        if (Bindings.TryGetValue(bindingCall.Name, out var binding))
        {
            _ = bindingCall.CallAsync(binding);
        }
    }

    private void Channel_Route(object sender, Route route) => _ = OnRouteAsync(route).ConfigureAwait(false);

    private async Task OnRouteAsync(Route route)
    {
        route._context = Context;
        foreach (var routeHandler in _routes.ToArray())
        {
            // If the page was closed we stall all requests right away.
            if (CloseWasCalled || Context.ClosingOrClosed)
            {
                return;
            }
            if (!routeHandler.Matches(route.Request.Url))
            {
                continue;
            }
            if (!_routes.Contains(routeHandler))
            {
                continue;
            }
            if (routeHandler.WillExpire())
            {
                _routes.Remove(routeHandler);
            }
            var handled = await routeHandler.HandleAsync(route).ConfigureAwait(false);
            if (_routes.Count == 0)
            {
                UpdateInterceptionAsync().IgnoreException();
            }
            if (handled)
            {
                return;
            }
        }

        await Context.OnRouteAsync(route).ConfigureAwait(false);
    }

    private async Task OnWebSocketRouteAsync(WebSocketRoute webSocketRoute)
    {
        var routeHandler = _webSocketRoutes.Find(route => route.Matches(webSocketRoute.Url));
        if (routeHandler != null)
        {
            await routeHandler.HandleAsync(webSocketRoute).ConfigureAwait(false);
        }
        else
        {
            await Context.OnWebSocketRouteAsync(webSocketRoute).ConfigureAwait(false);
        }
    }

    private void Channel_FrameDetached(object sender, IFrame args)
    {
        var frame = (Frame)args;
        _frames.Remove(frame);
        frame.IsDetached = true;
        frame.ParentFrame?._childFrames?.Remove(frame);
        FrameDetached?.Invoke(this, args);
    }

    private void Channel_FrameAttached(object sender, IFrame args)
    {
        var frame = (Frame)args;
        frame.Page = this;
        _frames.Add(frame);
        frame.ParentFrame?._childFrames?.Add(frame);
        FrameAttached?.Invoke(this, args);
    }

    private async Task InnerExposeBindingAsync(string name, Delegate callback, bool handle = false)
    {
        if (Bindings.ContainsKey(name))
        {
            throw new PlaywrightException($"Function \"{name}\" has been already registered");
        }

        Bindings.Add(name, callback);

        await SendMessageToServerAsync(
            "exposeBinding",
            new Dictionary<string, object?>
            {
                ["name"] = name,
                ["needsHandle"] = handle,
            }).ConfigureAwait(false);
    }

    private Video ForceVideo() => _video ??= new(this, _connection);

    private FrameSetInputFilesOptions? Map(PageSetInputFilesOptions? options)
    {
        if (options == null)
        {
            return null;
        }

        return new()
        {
            Timeout = options.Timeout,
            Strict = options.Strict,
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public async Task RouteFromHARAsync(string har, PageRouteFromHAROptions? options = null)
    {
        if (options?.Update == true)
        {
            await Context.RecordIntoHarAsync(
                har,
                this,
                new()
                {
                    NotFound = options.NotFound,
                    Url = options.Url,
                    UrlString = options.UrlString,
                    UrlRegex = options.UrlRegex,
                    Update = options.Update,
                    UpdateContent = options.UpdateContent,
                    UpdateMode = options.UpdateMode,
                },
                null).ConfigureAwait(false);
            return;
        }
        var harRouter = await HarRouter.CreateAsync(_connection.LocalUtils!, har, options?.NotFound ?? HarNotFound.Abort, new()
        {
            UrlRegex = options?.UrlRegex,
            Url = options?.Url,
            UrlString = options?.UrlString,
        }).ConfigureAwait(false);
        _harRouters.Add(harRouter);
        await harRouter.AddPageRouteAsync(this).ConfigureAwait(false);
    }

    private void DisposeHarRouters()
    {
        foreach (var router in _harRouters)
        {
            router.Dispose();
        }
        _harRouters.Clear();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByAltText(string text, PageGetByAltTextOptions? options = null)
        => MainFrame.GetByAltText(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByAltText(Regex text, PageGetByAltTextOptions? options = null)
        => MainFrame.GetByAltText(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByLabel(string text, PageGetByLabelOptions? options = null)
        => MainFrame.GetByLabel(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByLabel(Regex text, PageGetByLabelOptions? options = null)
        => MainFrame.GetByLabel(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByPlaceholder(string text, PageGetByPlaceholderOptions? options = null)
        => MainFrame.GetByPlaceholder(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByPlaceholder(Regex text, PageGetByPlaceholderOptions? options = null)
        => MainFrame.GetByPlaceholder(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByRole(AriaRole role, PageGetByRoleOptions? options = null)
        => Locator(Core.Locator.GetByRoleSelector(role, new(options)));

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByTestId(string testId)
        => MainFrame.GetByTestId(testId);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByTestId(Regex testId)
        => MainFrame.GetByTestId(testId);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByText(string text, PageGetByTextOptions? options = null)
        => MainFrame.GetByText(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByText(Regex text, PageGetByTextOptions? options = null)
        => MainFrame.GetByText(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByTitle(string text, PageGetByTitleOptions? options = null)
        => MainFrame.GetByTitle(text, new() { Exact = options?.Exact });

    [MethodImpl(MethodImplOptions.NoInlining)]
    public ILocator GetByTitle(Regex text, PageGetByTitleOptions? options = null)
        => MainFrame.GetByTitle(text, new() { Exact = options?.Exact });

    public Task AddLocatorHandlerAsync(ILocator locator, Func<Task> handler, PageAddLocatorHandlerOptions? options = null)
        => AddLocatorHandlerImplAsync(locator, handler, options);

    public Task AddLocatorHandlerAsync(ILocator locator, Func<ILocator, Task> handler, PageAddLocatorHandlerOptions? options = null)
        => AddLocatorHandlerImplAsync(locator, handler, options);

    private async Task AddLocatorHandlerImplAsync(ILocator locator, object handler, PageAddLocatorHandlerOptions? options = null)
    {
        if (((Locator)locator)._frame != MainFrame)
        {
            throw new PlaywrightException("Locator must belong to the main frame of this page");
        }
        if (options?.Times == 0)
        {
            return;
        }
        var response = await SendMessageToServerAsync("registerLocatorHandler", new Dictionary<string, object?>
        {
            ["selector"] = ((Locator)locator)._selector,
            ["noWaitAfter"] = options?.NoWaitAfter,
        }).ConfigureAwait(false);

        _locatorHandlers.Add(response.Value.GetProperty("uid").GetInt32(), new LocatorHandler((Locator)locator, handler, options?.Times));
    }

    private async Task Channel_LocatorHandlerTriggeredAsync(int uid)
    {
        var remove = false;
        try
        {
            if (_locatorHandlers.TryGetValue(uid, out var handler))
            {
                if (handler.Times != 0)
                {
                    if (handler.Times != null)
                    {
                        handler.Times--;
                    }
                    await handler.HandleAsync().ConfigureAwait(false);
                }
                remove = handler.Times == 0;
            }
        }
        finally
        {
            if (remove)
            {
                _locatorHandlers.Remove(uid);
            }
            SendMessageToServerAsync("resolveLocatorHandlerNoReply", new Dictionary<string, object?>
            {
                ["uid"] = uid,
                ["remove"] = remove,
            }).IgnoreException();
        }
    }

    public async Task RemoveLocatorHandlerAsync(ILocator locator)
    {
        foreach (KeyValuePair<int, LocatorHandler> entry in _locatorHandlers)
        {
            var (uid, data) = (entry.Key, entry.Value);
            if (data.Locator.EqualLocator((Locator)locator))
            {
                _locatorHandlers.Remove(uid);
                try
                {
                    await SendMessageToServerAsync("unregisterLocatorHandler", new Dictionary<string, object?>
                    {
                        ["uid"] = uid,
                    }).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteWebSocketAsync(string url, Action<IWebSocketRoute> handler)
        => RouteWebSocketAsync(url, null, null, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteWebSocketAsync(Regex url, Action<IWebSocketRoute> handler)
        => RouteWebSocketAsync(null, url, null, handler);

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Task RouteWebSocketAsync(Func<string, bool> url, Action<IWebSocketRoute> handler)
        => RouteWebSocketAsync(null, null, url, handler);

    private Task RouteWebSocketAsync(string? globMatch, Regex? urlRegex, Func<string, bool>? urlFunc, Delegate handler)
    {
        _webSocketRoutes.Insert(0, new WebSocketRouteHandler()
        {
            urlMatcher = new URLMatch()
            {
                baseURL = Context.BaseURL,
                glob = globMatch,
                re = urlRegex,
                func = urlFunc,
                isWebSocketUrl = true,
            },
            Handler = handler,
        });
        return UpdateWebSocketInterceptionAsync();
    }

    private async Task UpdateWebSocketInterceptionAsync()
    {
        var patterns = WebSocketRouteHandler.PrepareInterceptionPatterns(_webSocketRoutes);
        await SendMessageToServerAsync("setWebSocketInterceptionPatterns", new Dictionary<string, object?>
        {
            ["patterns"] = patterns,
        }).ConfigureAwait(false);
    }
}

internal class LocatorHandler
{
    internal LocatorHandler(Locator locator, object handler, int? times)
    {
        Locator = locator;
        Handler = handler;
        Times = times;
    }

    internal Locator Locator { get; }

    private object Handler { get; }

    internal int? Times { get; set; }

    internal Task HandleAsync()
    {
        if (Handler is Func<Task> funcTask)
        {
            return funcTask();
        }
        if (Handler is Func<ILocator, Task> funcLocatorTask)
        {
            return funcLocatorTask(Locator);
        }
        throw new PlaywrightException("Locator handler must be a Func<Task> or Func<ILocator, Task>");
    }
}
