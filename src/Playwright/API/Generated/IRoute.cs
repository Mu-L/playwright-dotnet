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

using System.Threading.Tasks;

namespace Microsoft.Playwright;

/// <summary>
/// <para>
/// Whenever a network route is set up with <see cref="IPage.RouteAsync"/> or <see cref="IBrowserContext.RouteAsync"/>,
/// the <c>Route</c> object allows to handle the route.
/// </para>
/// <para>Learn more about <a href="https://playwright.dev/dotnet/docs/network">networking</a>.</para>
/// </summary>
public partial interface IRoute
{
    /// <summary><para>Aborts the route's request.</para></summary>
    /// <param name="errorCode">
    /// Optional error code. Defaults to <c>failed</c>, could be one of the following:
    /// <list type="bullet">
    /// <item><description><c>'aborted'</c> - An operation was aborted (due to user action)</description></item>
    /// <item><description>
    /// <c>'accessdenied'</c> - Permission to access a resource, other than the network,
    /// was denied
    /// </description></item>
    /// <item><description>
    /// <c>'addressunreachable'</c> - The IP address is unreachable. This usually means
    /// that there is no route to the specified host or network.
    /// </description></item>
    /// <item><description><c>'blockedbyclient'</c> - The client chose to block the request.</description></item>
    /// <item><description>
    /// <c>'blockedbyresponse'</c> - The request failed because the response was delivered
    /// along with requirements which are not met ('X-Frame-Options' and 'Content-Security-Policy'
    /// ancestor checks, for instance).
    /// </description></item>
    /// <item><description>
    /// <c>'connectionaborted'</c> - A connection timed out as a result of not receiving
    /// an ACK for data sent.
    /// </description></item>
    /// <item><description><c>'connectionclosed'</c> - A connection was closed (corresponding to a TCP FIN).</description></item>
    /// <item><description><c>'connectionfailed'</c> - A connection attempt failed.</description></item>
    /// <item><description><c>'connectionrefused'</c> - A connection attempt was refused.</description></item>
    /// <item><description><c>'connectionreset'</c> - A connection was reset (corresponding to a TCP RST).</description></item>
    /// <item><description><c>'internetdisconnected'</c> - The Internet connection has been lost.</description></item>
    /// <item><description><c>'namenotresolved'</c> - The host name could not be resolved.</description></item>
    /// <item><description><c>'timedout'</c> - An operation timed out.</description></item>
    /// <item><description><c>'failed'</c> - A generic failure occurred.</description></item>
    /// </list>
    /// </param>
    Task AbortAsync(string? errorCode = default);

    /// <summary>
    /// <para>Sends route's request to the network with optional overrides.</para>
    /// <para>**Usage**</para>
    /// <code>
    /// await page.RouteAsync("**/*", async route =&gt;<br/>
    /// {<br/>
    ///     var headers = new Dictionary&lt;string, string&gt;(route.Request.Headers) { { "foo", "bar" } };<br/>
    ///     headers.Remove("origin");<br/>
    ///     await route.ContinueAsync(new() { Headers = headers });<br/>
    /// });
    /// </code>
    /// <para>**Details**</para>
    /// <para>
    /// The <see cref="IRoute.ContinueAsync"/> option applies to both the routed request
    /// and any redirects it initiates. However, <see cref="IRoute.ContinueAsync"/>, <see
    /// cref="IRoute.ContinueAsync"/>, and <see cref="IRoute.ContinueAsync"/> only apply
    /// to the original request and are not carried over to redirected requests.
    /// </para>
    /// <para>
    /// <see cref="IRoute.ContinueAsync"/> will immediately send the request to the network,
    /// other matching handlers won't be invoked. Use <see cref="IRoute.FallbackAsync"/>
    /// If you want next matching handler in the chain to be invoked.
    /// </para>
    /// <para>
    /// The <c>Cookie</c> header cannot be overridden using this method. If a value is provided,
    /// it will be ignored, and the cookie will be loaded from the browser's cookie store.
    /// To set custom cookies, use <see cref="IBrowserContext.AddCookiesAsync"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>Cookie</c> header cannot be overridden using this method. If a value is provided,
    /// it will be ignored, and the cookie will be loaded from the browser's cookie store.
    /// To set custom cookies, use <see cref="IBrowserContext.AddCookiesAsync"/>.
    /// </para>
    /// </remarks>
    /// <param name="options">Call options</param>
    Task ContinueAsync(RouteContinueOptions? options = default);

    /// <summary>
    /// <para>
    /// Continues route's request with optional overrides. The method is similar to <see
    /// cref="IRoute.ContinueAsync"/> with the difference that other matching handlers will
    /// be invoked before sending the request.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>
    /// When several routes match the given pattern, they run in the order opposite to their
    /// registration. That way the last registered route can always override all the previous
    /// ones. In the example below, request will be handled by the bottom-most handler first,
    /// then it'll fall back to the previous one and in the end will be aborted by the first
    /// registered route.
    /// </para>
    /// <code>
    /// await page.RouteAsync("**/*", route =&gt; {<br/>
    ///     // Runs last.<br/>
    ///     await route.AbortAsync();<br/>
    /// });<br/>
    /// <br/>
    /// await page.RouteAsync("**/*", route =&gt; {<br/>
    ///     // Runs second.<br/>
    ///     await route.FallbackAsync();<br/>
    /// });<br/>
    /// <br/>
    /// await page.RouteAsync("**/*", route =&gt; {<br/>
    ///     // Runs first.<br/>
    ///     await route.FallbackAsync();<br/>
    /// });
    /// </code>
    /// <para>
    /// Registering multiple routes is useful when you want separate handlers to handle
    /// different kinds of requests, for example API calls vs page resources or GET requests
    /// vs POST requests as in the example below.
    /// </para>
    /// <code>
    /// // Handle GET requests.<br/>
    /// await page.RouteAsync("**/*", route =&gt; {<br/>
    ///     if (route.Request.Method != "GET") {<br/>
    ///         await route.FallbackAsync();<br/>
    ///         return;<br/>
    ///     }<br/>
    ///     // Handling GET only.<br/>
    ///     // ...<br/>
    /// });<br/>
    /// <br/>
    /// // Handle POST requests.<br/>
    /// await page.RouteAsync("**/*", route =&gt; {<br/>
    ///     if (route.Request.Method != "POST") {<br/>
    ///         await route.FallbackAsync();<br/>
    ///         return;<br/>
    ///     }<br/>
    ///     // Handling POST only.<br/>
    ///     // ...<br/>
    /// });
    /// </code>
    /// <para>
    /// One can also modify request while falling back to the subsequent handler, that way
    /// intermediate route handler can modify url, method, headers and postData of the request.
    /// </para>
    /// <code>
    /// await page.RouteAsync("**/*", async route =&gt;<br/>
    /// {<br/>
    ///     var headers = new Dictionary&lt;string, string&gt;(route.Request.Headers) { { "foo", "foo-value" } };<br/>
    ///     headers.Remove("bar");<br/>
    ///     await route.FallbackAsync(new() { Headers = headers });<br/>
    /// });
    /// </code>
    /// <para>
    /// Use <see cref="IRoute.ContinueAsync"/> to immediately send the request to the network,
    /// other matching handlers won't be invoked in that case.
    /// </para>
    /// </summary>
    /// <param name="options">Call options</param>
    Task FallbackAsync(RouteFallbackOptions? options = default);

    /// <summary>
    /// <para>
    /// Performs the request and fetches result without fulfilling it, so that the response
    /// could be modified and then fulfilled.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// await page.RouteAsync("https://dog.ceo/api/breeds/list/all", async route =&gt;<br/>
    /// {<br/>
    ///     var response = await route.FetchAsync();<br/>
    ///     dynamic json = await response.JsonAsync();<br/>
    ///     json.message.big_red_dog = new string[] {};<br/>
    ///     await route.FulfillAsync(new() { Response = response, Json = json });<br/>
    /// });
    /// </code>
    /// <para>**Details**</para>
    /// <para>
    /// Note that <see cref="IRoute.FetchAsync"/> option will apply to the fetched request
    /// as well as any redirects initiated by it. If you want to only apply <see cref="IRoute.FetchAsync"/>
    /// to the original request, but not to redirects, look into <see cref="IRoute.ContinueAsync"/>
    /// instead.
    /// </para>
    /// </summary>
    /// <param name="options">Call options</param>
    Task<IAPIResponse> FetchAsync(RouteFetchOptions? options = default);

    /// <summary>
    /// <para>Fulfills route's request with given response.</para>
    /// <para>**Usage**</para>
    /// <para>An example of fulfilling all requests with 404 responses:</para>
    /// <code>
    /// await page.RouteAsync("**/*", route =&gt; route.FulfillAsync(new ()<br/>
    /// {<br/>
    ///     Status = 404,<br/>
    ///     ContentType = "text/plain",<br/>
    ///     Body = "Not Found!"<br/>
    /// }));
    /// </code>
    /// <para>An example of serving static file:</para>
    /// <code>await page.RouteAsync("**/xhr_endpoint", route =&gt; route.FulfillAsync(new() { Path = "mock_data.json" }));</code>
    /// </summary>
    /// <param name="options">Call options</param>
    Task FulfillAsync(RouteFulfillOptions? options = default);

    /// <summary><para>A request to be routed.</para></summary>
    IRequest Request { get; }
}
