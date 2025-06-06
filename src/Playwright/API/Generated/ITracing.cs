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
/// API for collecting and saving Playwright traces. Playwright traces can be opened
/// in <a href="https://playwright.dev/dotnet/docs/trace-viewer">Trace Viewer</a> after
/// Playwright script runs.
/// </para>
/// <para>
/// You probably want to <a href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enable
/// tracing in your config file</a> instead of using <c>context.tracing</c>.
/// </para>
/// <para>
/// The <c>context.tracing</c> API captures browser operations and network activity,
/// but it doesn't record test assertions (like <c>expect</c> calls). We recommend <a
/// href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enabling
/// tracing through Playwright Test configuration</a>, which includes those assertions
/// and provides a more complete trace for debugging test failures.
/// </para>
/// <para>
/// Start recording a trace before performing actions. At the end, stop tracing and
/// save it to a file.
/// </para>
/// <code>
/// using var playwright = await Playwright.CreateAsync();<br/>
/// var browser = await playwright.Chromium.LaunchAsync();<br/>
/// await using var context = await browser.NewContextAsync();<br/>
/// await context.Tracing.StartAsync(new()<br/>
/// {<br/>
///   Screenshots = true,<br/>
///   Snapshots = true<br/>
/// });<br/>
/// var page = await context.NewPageAsync();<br/>
/// await page.GotoAsync("https://playwright.dev");<br/>
/// await context.Tracing.StopAsync(new()<br/>
/// {<br/>
///   Path = "trace.zip"<br/>
/// });
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// You probably want to <a href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enable
/// tracing in your config file</a> instead of using <c>context.tracing</c>.  The <c>context.tracing</c>
/// API captures browser operations and network activity, but it doesn't record test
/// assertions (like <c>expect</c> calls). We recommend <a href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enabling
/// tracing through Playwright Test configuration</a>, which includes those assertions
/// and provides a more complete trace for debugging test failures.
/// </para>
/// </remarks>
public partial interface ITracing
{
    /// <summary>
    /// <para>Start tracing.</para>
    /// <para>
    /// You probably want to <a href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enable
    /// tracing in your config file</a> instead of using <c>Tracing.start</c>.
    /// </para>
    /// <para>
    /// The <c>context.tracing</c> API captures browser operations and network activity,
    /// but it doesn't record test assertions (like <c>expect</c> calls). We recommend <a
    /// href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enabling
    /// tracing through Playwright Test configuration</a>, which includes those assertions
    /// and provides a more complete trace for debugging test failures.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// using var playwright = await Playwright.CreateAsync();<br/>
    /// var browser = await playwright.Chromium.LaunchAsync();<br/>
    /// await using var context = await browser.NewContextAsync();<br/>
    /// await context.Tracing.StartAsync(new()<br/>
    /// {<br/>
    ///   Screenshots = true,<br/>
    ///   Snapshots = true<br/>
    /// });<br/>
    /// var page = await context.NewPageAsync();<br/>
    /// await page.GotoAsync("https://playwright.dev");<br/>
    /// await context.Tracing.StopAsync(new()<br/>
    /// {<br/>
    ///   Path = "trace.zip"<br/>
    /// });
    /// </code>
    /// </summary>
    /// <remarks>
    /// <para>
    /// You probably want to <a href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enable
    /// tracing in your config file</a> instead of using <c>Tracing.start</c>.  The <c>context.tracing</c>
    /// API captures browser operations and network activity, but it doesn't record test
    /// assertions (like <c>expect</c> calls). We recommend <a href="https://playwright.dev/docs/api/class-testoptions#test-options-trace">enabling
    /// tracing through Playwright Test configuration</a>, which includes those assertions
    /// and provides a more complete trace for debugging test failures.
    /// </para>
    /// </remarks>
    /// <param name="options">Call options</param>
    Task StartAsync(TracingStartOptions? options = default);

    /// <summary>
    /// <para>
    /// Start a new trace chunk. If you'd like to record multiple traces on the same <see
    /// cref="IBrowserContext"/>, use <see cref="ITracing.StartAsync"/> once, and then create
    /// multiple trace chunks with <see cref="ITracing.StartChunkAsync"/> and <see cref="ITracing.StopChunkAsync"/>.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// using var playwright = await Playwright.CreateAsync();<br/>
    /// var browser = await playwright.Chromium.LaunchAsync();<br/>
    /// await using var context = await browser.NewContextAsync();<br/>
    /// await context.Tracing.StartAsync(new()<br/>
    /// {<br/>
    ///   Screenshots = true,<br/>
    ///   Snapshots = true<br/>
    /// });<br/>
    /// var page = await context.NewPageAsync();<br/>
    /// await page.GotoAsync("https://playwright.dev");<br/>
    /// <br/>
    /// await context.Tracing.StartChunkAsync();<br/>
    /// await page.GetByText("Get Started").ClickAsync();<br/>
    /// // Everything between StartChunkAsync and StopChunkAsync will be recorded in the trace.<br/>
    /// await context.Tracing.StopChunkAsync(new()<br/>
    /// {<br/>
    ///   Path = "trace1.zip"<br/>
    /// });<br/>
    /// <br/>
    /// await context.Tracing.StartChunkAsync();<br/>
    /// await page.GotoAsync("http://example.com");<br/>
    /// // Save a second trace file with different actions.<br/>
    /// await context.Tracing.StopChunkAsync(new()<br/>
    /// {<br/>
    ///   Path = "trace2.zip"<br/>
    /// });
    /// </code>
    /// </summary>
    /// <param name="options">Call options</param>
    Task StartChunkAsync(TracingStartChunkOptions? options = default);

    /// <summary>
    /// <para>Use <c>test.step</c> instead when available.</para>
    /// <para>
    /// Creates a new group within the trace, assigning any subsequent API calls to this
    /// group, until <see cref="ITracing.GroupEndAsync"/> is called. Groups can be nested
    /// and will be visible in the trace viewer.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // All actions between GroupAsync and GroupEndAsync<br/>
    /// // will be shown in the trace viewer as a group.<br/>
    /// await Page.Context.Tracing.GroupAsync("Open Playwright.dev &gt; API");<br/>
    /// await Page.GotoAsync("https://playwright.dev/");<br/>
    /// await Page.GetByRole(AriaRole.Link, new() { Name = "API" }).ClickAsync();<br/>
    /// await Page.Context.Tracing.GroupEndAsync();
    /// </code>
    /// </summary>
    /// <remarks><para>Use <c>test.step</c> instead when available.</para></remarks>
    /// <param name="name">Group name shown in the trace viewer.</param>
    /// <param name="options">Call options</param>
    Task GroupAsync(string name, TracingGroupOptions? options = default);

    /// <summary><para>Closes the last group created by <see cref="ITracing.GroupAsync"/>.</para></summary>
    Task GroupEndAsync();

    /// <summary><para>Stop tracing.</para></summary>
    /// <param name="options">Call options</param>
    Task StopAsync(TracingStopOptions? options = default);

    /// <summary>
    /// <para>
    /// Stop the trace chunk. See <see cref="ITracing.StartChunkAsync"/> for more details
    /// about multiple trace chunks.
    /// </para>
    /// </summary>
    /// <param name="options">Call options</param>
    Task StopChunkAsync(TracingStopChunkOptions? options = default);
}
