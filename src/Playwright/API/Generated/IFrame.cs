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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Playwright;

/// <summary>
/// <para>
/// At every point of time, page exposes its current frame tree via the <see cref="IPage.MainFrame"/>
/// and <see cref="IFrame.ChildFrames"/> methods.
/// </para>
/// <para>
/// <see cref="IFrame"/> object's lifecycle is controlled by three events, dispatched
/// on the page object:
/// </para>
/// <list type="bullet">
/// <item><description>
/// <see cref="IPage.FrameAttached"/> - fired when the frame gets attached to the page.
/// A Frame can be attached to the page only once.
/// </description></item>
/// <item><description>
/// <see cref="IPage.FrameNavigated"/> - fired when the frame commits navigation to
/// a different URL.
/// </description></item>
/// <item><description>
/// <see cref="IPage.FrameDetached"/> - fired when the frame gets detached from the
/// page.  A Frame can be detached from the page only once.
/// </description></item>
/// </list>
/// <para>An example of dumping frame tree:</para>
/// <code>
/// using Microsoft.Playwright;<br/>
/// using System;<br/>
/// using System.Threading.Tasks;<br/>
/// <br/>
/// class FrameExamples<br/>
/// {<br/>
///     public static async Task Main()<br/>
///     {<br/>
///         using var playwright = await Playwright.CreateAsync();<br/>
///         await using var browser = await playwright.Firefox.LaunchAsync();<br/>
///         var page = await browser.NewPageAsync();<br/>
/// <br/>
///         await page.GotoAsync("https://www.bing.com");<br/>
///         DumpFrameTree(page.MainFrame, string.Empty);<br/>
///     }<br/>
/// <br/>
///     private static void DumpFrameTree(IFrame frame, string indent)<br/>
///     {<br/>
///         Console.WriteLine($"{indent}{frame.Url}");<br/>
///         foreach (var child in frame.ChildFrames)<br/>
///             DumpFrameTree(child, indent + " ");<br/>
///     }<br/>
/// }
/// </code>
/// </summary>
public partial interface IFrame
{
    /// <summary>
    /// <para>
    /// Returns the added tag when the script's onload fires or when the script content
    /// was injected into frame.
    /// </para>
    /// <para>Adds a <c>&lt;script&gt;</c> tag into the page with the desired url or content.</para>
    /// </summary>
    /// <param name="options">Call options</param>
    Task<IElementHandle> AddScriptTagAsync(FrameAddScriptTagOptions? options = default);

    /// <summary>
    /// <para>
    /// Returns the added tag when the stylesheet's onload fires or when the CSS content
    /// was injected into frame.
    /// </para>
    /// <para>
    /// Adds a <c>&lt;link rel="stylesheet"&gt;</c> tag into the page with the desired url
    /// or a <c>&lt;style type="text/css"&gt;</c> tag with the content.
    /// </para>
    /// </summary>
    /// <param name="options">Call options</param>
    Task<IElementHandle> AddStyleTagAsync(FrameAddStyleTagOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.CheckAsync"/> instead. Read more about <a
    /// href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method checks an element matching <see cref="IFrame.CheckAsync"/> by performing
    /// the following steps:
    /// </para>
    /// <list type="ordinal">
    /// <item><description>
    /// Find an element matching <see cref="IFrame.CheckAsync"/>. If there is none, wait
    /// until a matching element is attached to the DOM.
    /// </description></item>
    /// <item><description>
    /// Ensure that matched element is a checkbox or a radio input. If not, this method
    /// throws. If the element is already checked, this method returns immediately.
    /// </description></item>
    /// <item><description>
    /// Wait for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks on the matched element, unless <see cref="IFrame.CheckAsync"/> option is
    /// set. If the element is detached during the checks, the whole action is retried.
    /// </description></item>
    /// <item><description>Scroll the element into view if needed.</description></item>
    /// <item><description>Use <see cref="IPage.Mouse"/> to click in the center of the element.</description></item>
    /// <item><description>Ensure that the element is now checked. If not, this method throws.</description></item>
    /// </list>
    /// <para>
    /// When all steps combined have not finished during the specified <see cref="IFrame.CheckAsync"/>,
    /// this method throws a <see cref="TimeoutException"/>. Passing zero timeout disables
    /// this.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task CheckAsync(string selector, FrameCheckOptions? options = default);

    IReadOnlyList<IFrame> ChildFrames { get; }

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.ClickAsync"/> instead. Read more about <a
    /// href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method clicks an element matching <see cref="IFrame.ClickAsync"/> by performing
    /// the following steps:
    /// </para>
    /// <list type="ordinal">
    /// <item><description>
    /// Find an element matching <see cref="IFrame.ClickAsync"/>. If there is none, wait
    /// until a matching element is attached to the DOM.
    /// </description></item>
    /// <item><description>
    /// Wait for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks on the matched element, unless <see cref="IFrame.ClickAsync"/> option is
    /// set. If the element is detached during the checks, the whole action is retried.
    /// </description></item>
    /// <item><description>Scroll the element into view if needed.</description></item>
    /// <item><description>
    /// Use <see cref="IPage.Mouse"/> to click in the center of the element, or the specified
    /// <see cref="IFrame.ClickAsync"/>.
    /// </description></item>
    /// <item><description>
    /// Wait for initiated navigations to either succeed or fail, unless <see cref="IFrame.ClickAsync"/>
    /// option is set.
    /// </description></item>
    /// </list>
    /// <para>
    /// When all steps combined have not finished during the specified <see cref="IFrame.ClickAsync"/>,
    /// this method throws a <see cref="TimeoutException"/>. Passing zero timeout disables
    /// this.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task ClickAsync(string selector, FrameClickOptions? options = default);

    /// <summary><para>Gets the full HTML contents of the frame, including the doctype.</para></summary>
    Task<string> ContentAsync();

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.DblClickAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method double clicks an element matching <see cref="IFrame.DblClickAsync"/>
    /// by performing the following steps:
    /// </para>
    /// <list type="ordinal">
    /// <item><description>
    /// Find an element matching <see cref="IFrame.DblClickAsync"/>. If there is none, wait
    /// until a matching element is attached to the DOM.
    /// </description></item>
    /// <item><description>
    /// Wait for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks on the matched element, unless <see cref="IFrame.DblClickAsync"/> option
    /// is set. If the element is detached during the checks, the whole action is retried.
    /// </description></item>
    /// <item><description>Scroll the element into view if needed.</description></item>
    /// <item><description>
    /// Use <see cref="IPage.Mouse"/> to double click in the center of the element, or the
    /// specified <see cref="IFrame.DblClickAsync"/>. if the first click of the <c>dblclick()</c>
    /// triggers a navigation event, this method will throw.
    /// </description></item>
    /// </list>
    /// <para>
    /// When all steps combined have not finished during the specified <see cref="IFrame.DblClickAsync"/>,
    /// this method throws a <see cref="TimeoutException"/>. Passing zero timeout disables
    /// this.
    /// </para>
    /// <para>
    /// <c>frame.dblclick()</c> dispatches two <c>click</c> events and a single <c>dblclick</c>
    /// event.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>frame.dblclick()</c> dispatches two <c>click</c> events and a single <c>dblclick</c>
    /// event.
    /// </para>
    /// </remarks>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task DblClickAsync(string selector, FrameDblClickOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.DispatchEventAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// The snippet below dispatches the <c>click</c> event on the element. Regardless of
    /// the visibility state of the element, <c>click</c> is dispatched. This is equivalent
    /// to calling <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement/click">element.click()</a>.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>await frame.DispatchEventAsync("button#submit", "click");</code>
    /// <para>
    /// Under the hood, it creates an instance of an event based on the given <see cref="IFrame.DispatchEventAsync"/>,
    /// initializes it with <see cref="IFrame.DispatchEventAsync"/> properties and dispatches
    /// it on the element. Events are <c>composed</c>, <c>cancelable</c> and bubble by default.
    /// </para>
    /// <para>
    /// Since <see cref="IFrame.DispatchEventAsync"/> is event-specific, please refer to
    /// the events documentation for the lists of initial properties:
    /// </para>
    /// <list type="bullet">
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/DeviceMotionEvent/DeviceMotionEvent">DeviceMotionEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/DeviceOrientationEvent/DeviceOrientationEvent">DeviceOrientationEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/DragEvent/DragEvent">DragEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/Event/Event">Event</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/FocusEvent/FocusEvent">FocusEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/KeyboardEvent">KeyboardEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/MouseEvent">MouseEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/PointerEvent/PointerEvent">PointerEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/TouchEvent/TouchEvent">TouchEvent</a></description></item>
    /// <item><description><a href="https://developer.mozilla.org/en-US/docs/Web/API/WheelEvent/WheelEvent">WheelEvent</a></description></item>
    /// </list>
    /// <para>
    /// You can also specify <c>JSHandle</c> as the property value if you want live objects
    /// to be passed into the event:
    /// </para>
    /// <code>
    /// // Note you can only create DataTransfer in Chromium and Firefox<br/>
    /// var dataTransfer = await frame.EvaluateHandleAsync("() =&gt; new DataTransfer()");<br/>
    /// await frame.DispatchEventAsync("#source", "dragstart", new { dataTransfer });
    /// </code>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="type">DOM event type: <c>"click"</c>, <c>"dragstart"</c>, etc.</param>
    /// <param name="eventInit">Optional event-specific initialization properties.</param>
    /// <param name="options">Call options</param>
    Task DispatchEventAsync(string selector, string type, object? eventInit = default, FrameDispatchEventOptions? options = default);

    /// <param name="source">
    /// A selector to search for an element to drag. If there are multiple elements satisfying
    /// the selector, the first will be used.
    /// </param>
    /// <param name="target">
    /// A selector to search for an element to drop onto. If there are multiple elements
    /// satisfying the selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task DragAndDropAsync(string source, string target, FrameDragAndDropOptions? options = default);

    /// <summary>
    /// <para>
    /// This method does not wait for the element to pass the actionability checks and therefore
    /// can lead to the flaky tests. Use <see cref="ILocator.EvaluateAsync"/>, other <see
    /// cref="ILocator"/> helper methods or web-first assertions instead.
    /// </para>
    /// <para>Returns the return value of <see cref="IFrame.EvalOnSelectorAsync"/>.</para>
    /// <para>
    /// The method finds an element matching the specified selector within the frame and
    /// passes it as a first argument to <see cref="IFrame.EvalOnSelectorAsync"/>. If no
    /// elements match the selector, the method throws an error.
    /// </para>
    /// <para>
    /// If <see cref="IFrame.EvalOnSelectorAsync"/> returns a <see cref="Task"/>, then <see
    /// cref="IFrame.EvalOnSelectorAsync"/> would wait for the promise to resolve and return
    /// its value.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// var searchValue = await frame.EvalOnSelectorAsync&lt;string&gt;("#search", "el =&gt; el.value");<br/>
    /// var preloadHref = await frame.EvalOnSelectorAsync&lt;string&gt;("link[rel=preload]", "el =&gt; el.href");<br/>
    /// var html = await frame.EvalOnSelectorAsync(".main-container", "(e, suffix) =&gt; e.outerHTML + suffix", "hello");
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="expression">
    /// JavaScript expression to be evaluated in the browser context. If the expression
    /// evaluates to a function, the function is automatically invoked.
    /// </param>
    /// <param name="arg">Optional argument to pass to <see cref="IFrame.EvalOnSelectorAsync"/>.</param>
    /// <param name="options">Call options</param>
    Task<T> EvalOnSelectorAsync<T>(string selector, string expression, object? arg = default, FrameEvalOnSelectorOptions? options = default);

    /// <summary>
    /// <para>
    /// In most cases, <see cref="ILocator.EvaluateAllAsync"/>, other <see cref="ILocator"/>
    /// helper methods and web-first assertions do a better job.
    /// </para>
    /// <para>Returns the return value of <see cref="IFrame.EvalOnSelectorAllAsync"/>.</para>
    /// <para>
    /// The method finds all elements matching the specified selector within the frame and
    /// passes an array of matched elements as a first argument to <see cref="IFrame.EvalOnSelectorAllAsync"/>.
    /// </para>
    /// <para>
    /// If <see cref="IFrame.EvalOnSelectorAllAsync"/> returns a <see cref="Task"/>, then
    /// <see cref="IFrame.EvalOnSelectorAllAsync"/> would wait for the promise to resolve
    /// and return its value.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>var divsCount = await frame.EvalOnSelectorAllAsync&lt;bool&gt;("div", "(divs, min) =&gt; divs.length &gt;= min", 10);</code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="expression">
    /// JavaScript expression to be evaluated in the browser context. If the expression
    /// evaluates to a function, the function is automatically invoked.
    /// </param>
    /// <param name="arg">Optional argument to pass to <see cref="IFrame.EvalOnSelectorAllAsync"/>.</param>
    Task<T> EvalOnSelectorAllAsync<T>(string selector, string expression, object? arg = default);

    /// <summary>
    /// <para>Returns the return value of <see cref="IFrame.EvaluateAsync"/>.</para>
    /// <para>
    /// If the function passed to the <see cref="IFrame.EvaluateAsync"/> returns a <see
    /// cref="Task"/>, then <see cref="IFrame.EvaluateAsync"/> would wait for the promise
    /// to resolve and return its value.
    /// </para>
    /// <para>
    /// If the function passed to the <see cref="IFrame.EvaluateAsync"/> returns a non-<see
    /// cref="Serializable"/> value, then <see cref="IFrame.EvaluateAsync"/> returns <c>undefined</c>.
    /// Playwright also supports transferring some additional values that are not serializable
    /// by <c>JSON</c>: <c>-0</c>, <c>NaN</c>, <c>Infinity</c>, <c>-Infinity</c>.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// var result = await frame.EvaluateAsync&lt;int&gt;("([x, y]) =&gt; Promise.resolve(x * y)", new[] { 7, 8 });<br/>
    /// Console.WriteLine(result);
    /// </code>
    /// <para>A string can also be passed in instead of a function.</para>
    /// <code>Console.WriteLine(await frame.EvaluateAsync&lt;int&gt;("1 + 2")); // prints "3"</code>
    /// <para>
    /// <see cref="IElementHandle"/> instances can be passed as an argument to the <see
    /// cref="IFrame.EvaluateAsync"/>:
    /// </para>
    /// <code>
    /// var bodyHandle = await frame.EvaluateAsync("document.body");<br/>
    /// var html = await frame.EvaluateAsync&lt;string&gt;("([body, suffix]) =&gt; body.innerHTML + suffix", new object [] { bodyHandle, "hello" });<br/>
    /// await bodyHandle.DisposeAsync();
    /// </code>
    /// </summary>
    /// <param name="expression">
    /// JavaScript expression to be evaluated in the browser context. If the expression
    /// evaluates to a function, the function is automatically invoked.
    /// </param>
    /// <param name="arg">Optional argument to pass to <see cref="IFrame.EvaluateAsync"/>.</param>
    Task<T> EvaluateAsync<T>(string expression, object? arg = default);

    /// <summary>
    /// <para>Returns the return value of <see cref="IFrame.EvaluateHandleAsync"/> as a <see cref="IJSHandle"/>.</para>
    /// <para>
    /// The only difference between <see cref="IFrame.EvaluateAsync"/> and <see cref="IFrame.EvaluateHandleAsync"/>
    /// is that <see cref="IFrame.EvaluateHandleAsync"/> returns <see cref="IJSHandle"/>.
    /// </para>
    /// <para>
    /// If the function, passed to the <see cref="IFrame.EvaluateHandleAsync"/>, returns
    /// a <see cref="Task"/>, then <see cref="IFrame.EvaluateHandleAsync"/> would wait for
    /// the promise to resolve and return its value.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // Handle for the window object.<br/>
    /// var aWindowHandle = await frame.EvaluateHandleAsync("() =&gt; Promise.resolve(window)");
    /// </code>
    /// <para>A string can also be passed in instead of a function.</para>
    /// <code>var docHandle = await frame.EvaluateHandleAsync("document"); // Handle for the `document`</code>
    /// <para><see cref="IJSHandle"/> instances can be passed as an argument to the <see cref="IFrame.EvaluateHandleAsync"/>:</para>
    /// <code>
    /// var handle = await frame.EvaluateHandleAsync("() =&gt; document.body");<br/>
    /// var resultHandle = await frame.EvaluateHandleAsync("([body, suffix]) =&gt; body.innerHTML + suffix", new object[] { handle, "hello" });<br/>
    /// Console.WriteLine(await resultHandle.JsonValueAsync&lt;string&gt;());<br/>
    /// await resultHandle.DisposeAsync();
    /// </code>
    /// </summary>
    /// <param name="expression">
    /// JavaScript expression to be evaluated in the browser context. If the expression
    /// evaluates to a function, the function is automatically invoked.
    /// </param>
    /// <param name="arg">Optional argument to pass to <see cref="IFrame.EvaluateHandleAsync"/>.</param>
    Task<IJSHandle> EvaluateHandleAsync(string expression, object? arg = default);

    /// <summary>
    /// <para>Use locator-based <see cref="ILocator.FillAsync"/> instead. Read more about <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.</para>
    /// <para>
    /// This method waits for an element matching <see cref="IFrame.FillAsync"/>, waits
    /// for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks, focuses the element, fills it and triggers an <c>input</c> event after filling.
    /// Note that you can pass an empty string to clear the input field.
    /// </para>
    /// <para>
    /// If the target element is not an <c>&lt;input&gt;</c>, <c>&lt;textarea&gt;</c> or
    /// <c>[contenteditable]</c> element, this method throws an error. However, if the element
    /// is inside the <c>&lt;label&gt;</c> element that has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// the control will be filled instead.
    /// </para>
    /// <para>To send fine-grained keyboard events, use <see cref="ILocator.PressSequentiallyAsync"/>.</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="value">
    /// Value to fill for the <c>&lt;input&gt;</c>, <c>&lt;textarea&gt;</c> or <c>[contenteditable]</c>
    /// element.
    /// </param>
    /// <param name="options">Call options</param>
    Task FillAsync(string selector, string value, FrameFillOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.FocusAsync"/> instead. Read more about <a
    /// href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method fetches an element with <see cref="IFrame.FocusAsync"/> and focuses
    /// it. If there's no element matching <see cref="IFrame.FocusAsync"/>, the method waits
    /// until a matching element appears in the DOM.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task FocusAsync(string selector, FrameFocusOptions? options = default);

    /// <summary>
    /// <para>
    /// Returns the <c>frame</c> or <c>iframe</c> element handle which corresponds to this
    /// frame.
    /// </para>
    /// <para>
    /// This is an inverse of <see cref="IElementHandle.ContentFrameAsync"/>. Note that
    /// returned handle actually belongs to the parent frame.
    /// </para>
    /// <para>
    /// This method throws an error if the frame has been detached before <c>frameElement()</c>
    /// returns.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// var frameElement = await frame.FrameElementAsync();<br/>
    /// var contentFrame = await frameElement.ContentFrameAsync();<br/>
    /// Console.WriteLine(frame == contentFrame); // -&gt; True
    /// </code>
    /// </summary>
    Task<IElementHandle> FrameElementAsync();

    /// <summary>
    /// <para>
    /// When working with iframes, you can create a frame locator that will enter the iframe
    /// and allow selecting elements in that iframe.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>
    /// Following snippet locates element with text "Submit" in the iframe with id <c>my-frame</c>,
    /// like <c>&lt;iframe id="my-frame"&gt;</c>:
    /// </para>
    /// <code>
    /// var locator = frame.FrameLocator("#my-iframe").GetByText("Submit");<br/>
    /// await locator.ClickAsync();
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to use when resolving DOM element.</param>
    IFrameLocator FrameLocator(string selector);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.GetAttributeAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>Returns element attribute value.</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="name">Attribute name to get the value for.</param>
    /// <param name="options">Call options</param>
    Task<string?> GetAttributeAsync(string selector, string name, FrameGetAttributeOptions? options = default);

    /// <summary>
    /// <para>Allows locating elements by their alt text.</para>
    /// <para>**Usage**</para>
    /// <para>For example, this method will find the image by alt text "Playwright logo":</para>
    /// <code>await page.GetByAltText("Playwright logo").ClickAsync();</code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByAltText(string text, FrameGetByAltTextOptions? options = default);

    /// <summary>
    /// <para>Allows locating elements by their alt text.</para>
    /// <para>**Usage**</para>
    /// <para>For example, this method will find the image by alt text "Playwright logo":</para>
    /// <code>await page.GetByAltText("Playwright logo").ClickAsync();</code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByAltText(Regex text, FrameGetByAltTextOptions? options = default);

    /// <summary>
    /// <para>
    /// Allows locating input elements by the text of the associated <c>&lt;label&gt;</c>
    /// or <c>aria-labelledby</c> element, or by the <c>aria-label</c> attribute.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>
    /// For example, this method will find inputs by label "Username" and "Password" in
    /// the following DOM:
    /// </para>
    /// <code>
    /// await page.GetByLabel("Username").FillAsync("john");<br/>
    /// await page.GetByLabel("Password").FillAsync("secret");
    /// </code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByLabel(string text, FrameGetByLabelOptions? options = default);

    /// <summary>
    /// <para>
    /// Allows locating input elements by the text of the associated <c>&lt;label&gt;</c>
    /// or <c>aria-labelledby</c> element, or by the <c>aria-label</c> attribute.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>
    /// For example, this method will find inputs by label "Username" and "Password" in
    /// the following DOM:
    /// </para>
    /// <code>
    /// await page.GetByLabel("Username").FillAsync("john");<br/>
    /// await page.GetByLabel("Password").FillAsync("secret");
    /// </code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByLabel(Regex text, FrameGetByLabelOptions? options = default);

    /// <summary>
    /// <para>Allows locating input elements by the placeholder text.</para>
    /// <para>**Usage**</para>
    /// <para>For example, consider the following DOM structure.</para>
    /// <para>You can fill the input after locating it by the placeholder text:</para>
    /// <code>
    /// await page<br/>
    ///     .GetByPlaceholder("name@example.com")<br/>
    ///     .FillAsync("playwright@microsoft.com");
    /// </code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByPlaceholder(string text, FrameGetByPlaceholderOptions? options = default);

    /// <summary>
    /// <para>Allows locating input elements by the placeholder text.</para>
    /// <para>**Usage**</para>
    /// <para>For example, consider the following DOM structure.</para>
    /// <para>You can fill the input after locating it by the placeholder text:</para>
    /// <code>
    /// await page<br/>
    ///     .GetByPlaceholder("name@example.com")<br/>
    ///     .FillAsync("playwright@microsoft.com");
    /// </code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByPlaceholder(Regex text, FrameGetByPlaceholderOptions? options = default);

    /// <summary>
    /// <para>
    /// Allows locating elements by their <a href="https://www.w3.org/TR/wai-aria-1.2/#roles">ARIA
    /// role</a>, <a href="https://www.w3.org/TR/wai-aria-1.2/#aria-attributes">ARIA attributes</a>
    /// and <a href="https://w3c.github.io/accname/#dfn-accessible-name">accessible name</a>.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>Consider the following DOM structure.</para>
    /// <para>You can locate each element by it's implicit role:</para>
    /// <code>
    /// await Expect(Page<br/>
    ///     .GetByRole(AriaRole.Heading, new() { Name = "Sign up" }))<br/>
    ///     .ToBeVisibleAsync();<br/>
    /// <br/>
    /// await page<br/>
    ///     .GetByRole(AriaRole.Checkbox, new() { Name = "Subscribe" })<br/>
    ///     .CheckAsync();<br/>
    /// <br/>
    /// await page<br/>
    ///     .GetByRole(AriaRole.Button, new() {<br/>
    ///         NameRegex = new Regex("submit", RegexOptions.IgnoreCase)<br/>
    ///     })<br/>
    ///     .ClickAsync();
    /// </code>
    /// <para>**Details**</para>
    /// <para>
    /// Role selector **does not replace** accessibility audits and conformance tests, but
    /// rather gives early feedback about the ARIA guidelines.
    /// </para>
    /// <para>
    /// Many html elements have an implicitly <a href="https://w3c.github.io/html-aam/#html-element-role-mappings">defined
    /// role</a> that is recognized by the role selector. You can find all the <a href="https://www.w3.org/TR/wai-aria-1.2/#role_definitions">supported
    /// roles here</a>. ARIA guidelines **do not recommend** duplicating implicit roles
    /// and attributes by setting <c>role</c> and/or <c>aria-*</c> attributes to default
    /// values.
    /// </para>
    /// </summary>
    /// <param name="role">Required aria role.</param>
    /// <param name="options">Call options</param>
    ILocator GetByRole(AriaRole role, FrameGetByRoleOptions? options = default);

    /// <summary>
    /// <para>Locate element by the test id.</para>
    /// <para>**Usage**</para>
    /// <para>Consider the following DOM structure.</para>
    /// <para>You can locate the element by it's test id:</para>
    /// <code>await page.GetByTestId("directions").ClickAsync();</code>
    /// <para>**Details**</para>
    /// <para>
    /// By default, the <c>data-testid</c> attribute is used as a test id. Use <see cref="ISelectors.SetTestIdAttribute"/>
    /// to configure a different test id attribute if necessary.
    /// </para>
    /// </summary>
    /// <param name="testId">Id to locate the element by.</param>
    ILocator GetByTestId(string testId);

    /// <summary>
    /// <para>Locate element by the test id.</para>
    /// <para>**Usage**</para>
    /// <para>Consider the following DOM structure.</para>
    /// <para>You can locate the element by it's test id:</para>
    /// <code>await page.GetByTestId("directions").ClickAsync();</code>
    /// <para>**Details**</para>
    /// <para>
    /// By default, the <c>data-testid</c> attribute is used as a test id. Use <see cref="ISelectors.SetTestIdAttribute"/>
    /// to configure a different test id attribute if necessary.
    /// </para>
    /// </summary>
    /// <param name="testId">Id to locate the element by.</param>
    ILocator GetByTestId(Regex testId);

    /// <summary>
    /// <para>Allows locating elements that contain given text.</para>
    /// <para>
    /// See also <see cref="ILocator.Filter"/> that allows to match by another criteria,
    /// like an accessible role, and then filter by the text content.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>Consider the following DOM structure:</para>
    /// <para>You can locate by text substring, exact string, or a regular expression:</para>
    /// <code>
    /// // Matches &lt;span&gt;<br/>
    /// page.GetByText("world");<br/>
    /// <br/>
    /// // Matches first &lt;div&gt;<br/>
    /// page.GetByText("Hello world");<br/>
    /// <br/>
    /// // Matches second &lt;div&gt;<br/>
    /// page.GetByText("Hello", new() { Exact = true });<br/>
    /// <br/>
    /// // Matches both &lt;div&gt;s<br/>
    /// page.GetByText(new Regex("Hello"));<br/>
    /// <br/>
    /// // Matches second &lt;div&gt;<br/>
    /// page.GetByText(new Regex("^hello$", RegexOptions.IgnoreCase));
    /// </code>
    /// <para>**Details**</para>
    /// <para>
    /// Matching by text always normalizes whitespace, even with exact match. For example,
    /// it turns multiple spaces into one, turns line breaks into spaces and ignores leading
    /// and trailing whitespace.
    /// </para>
    /// <para>
    /// Input elements of the type <c>button</c> and <c>submit</c> are matched by their
    /// <c>value</c> instead of the text content. For example, locating by text <c>"Log
    /// in"</c> matches <c>&lt;input type=button value="Log in"&gt;</c>.
    /// </para>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByText(string text, FrameGetByTextOptions? options = default);

    /// <summary>
    /// <para>Allows locating elements that contain given text.</para>
    /// <para>
    /// See also <see cref="ILocator.Filter"/> that allows to match by another criteria,
    /// like an accessible role, and then filter by the text content.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>Consider the following DOM structure:</para>
    /// <para>You can locate by text substring, exact string, or a regular expression:</para>
    /// <code>
    /// // Matches &lt;span&gt;<br/>
    /// page.GetByText("world");<br/>
    /// <br/>
    /// // Matches first &lt;div&gt;<br/>
    /// page.GetByText("Hello world");<br/>
    /// <br/>
    /// // Matches second &lt;div&gt;<br/>
    /// page.GetByText("Hello", new() { Exact = true });<br/>
    /// <br/>
    /// // Matches both &lt;div&gt;s<br/>
    /// page.GetByText(new Regex("Hello"));<br/>
    /// <br/>
    /// // Matches second &lt;div&gt;<br/>
    /// page.GetByText(new Regex("^hello$", RegexOptions.IgnoreCase));
    /// </code>
    /// <para>**Details**</para>
    /// <para>
    /// Matching by text always normalizes whitespace, even with exact match. For example,
    /// it turns multiple spaces into one, turns line breaks into spaces and ignores leading
    /// and trailing whitespace.
    /// </para>
    /// <para>
    /// Input elements of the type <c>button</c> and <c>submit</c> are matched by their
    /// <c>value</c> instead of the text content. For example, locating by text <c>"Log
    /// in"</c> matches <c>&lt;input type=button value="Log in"&gt;</c>.
    /// </para>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByText(Regex text, FrameGetByTextOptions? options = default);

    /// <summary>
    /// <para>Allows locating elements by their title attribute.</para>
    /// <para>**Usage**</para>
    /// <para>Consider the following DOM structure.</para>
    /// <para>You can check the issues count after locating it by the title text:</para>
    /// <code>await Expect(Page.GetByTitle("Issues count")).toHaveText("25 issues");</code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByTitle(string text, FrameGetByTitleOptions? options = default);

    /// <summary>
    /// <para>Allows locating elements by their title attribute.</para>
    /// <para>**Usage**</para>
    /// <para>Consider the following DOM structure.</para>
    /// <para>You can check the issues count after locating it by the title text:</para>
    /// <code>await Expect(Page.GetByTitle("Issues count")).toHaveText("25 issues");</code>
    /// </summary>
    /// <param name="text">Text to locate the element for.</param>
    /// <param name="options">Call options</param>
    ILocator GetByTitle(Regex text, FrameGetByTitleOptions? options = default);

    /// <summary>
    /// <para>
    /// Returns the main resource response. In case of multiple redirects, the navigation
    /// will resolve with the response of the last redirect.
    /// </para>
    /// <para>The method will throw an error if:</para>
    /// <list type="bullet">
    /// <item><description>there's an SSL error (e.g. in case of self-signed certificates).</description></item>
    /// <item><description>target URL is invalid.</description></item>
    /// <item><description>the <see cref="IFrame.GotoAsync"/> is exceeded during navigation.</description></item>
    /// <item><description>the remote server does not respond or is unreachable.</description></item>
    /// <item><description>the main resource failed to load.</description></item>
    /// </list>
    /// <para>
    /// The method will not throw an error when any valid HTTP status code is returned by
    /// the remote server, including 404 "Not Found" and 500 "Internal Server Error".  The
    /// status code for such responses can be retrieved by calling <see cref="IResponse.Status"/>.
    /// </para>
    /// <para>
    /// The method either throws an error or returns a main resource response. The only
    /// exceptions are navigation to <c>about:blank</c> or navigation to the same URL with
    /// a different hash, which would succeed and return <c>null</c>.
    /// </para>
    /// <para>
    /// Headless mode doesn't support navigation to a PDF document. See the <a href="https://bugs.chromium.org/p/chromium/issues/detail?id=761295">upstream
    /// issue</a>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The method either throws an error or returns a main resource response. The only
    /// exceptions are navigation to <c>about:blank</c> or navigation to the same URL with
    /// a different hash, which would succeed and return <c>null</c>.
    /// </para>
    /// <para>
    /// Headless mode doesn't support navigation to a PDF document. See the <a href="https://bugs.chromium.org/p/chromium/issues/detail?id=761295">upstream
    /// issue</a>.
    /// </para>
    /// </remarks>
    /// <param name="url">URL to navigate frame to. The url should include scheme, e.g. <c>https://</c>.</param>
    /// <param name="options">Call options</param>
    Task<IResponse?> GotoAsync(string url, FrameGotoOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.HoverAsync"/> instead. Read more about <a
    /// href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method hovers over an element matching <see cref="IFrame.HoverAsync"/> by performing
    /// the following steps:
    /// </para>
    /// <list type="ordinal">
    /// <item><description>
    /// Find an element matching <see cref="IFrame.HoverAsync"/>. If there is none, wait
    /// until a matching element is attached to the DOM.
    /// </description></item>
    /// <item><description>
    /// Wait for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks on the matched element, unless <see cref="IFrame.HoverAsync"/> option is
    /// set. If the element is detached during the checks, the whole action is retried.
    /// </description></item>
    /// <item><description>Scroll the element into view if needed.</description></item>
    /// <item><description>
    /// Use <see cref="IPage.Mouse"/> to hover over the center of the element, or the specified
    /// <see cref="IFrame.HoverAsync"/>.
    /// </description></item>
    /// </list>
    /// <para>
    /// When all steps combined have not finished during the specified <see cref="IFrame.HoverAsync"/>,
    /// this method throws a <see cref="TimeoutException"/>. Passing zero timeout disables
    /// this.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task HoverAsync(string selector, FrameHoverOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.InnerHTMLAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>Returns <c>element.innerHTML</c>.</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<string> InnerHTMLAsync(string selector, FrameInnerHTMLOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.InnerTextAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>Returns <c>element.innerText</c>.</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<string> InnerTextAsync(string selector, FrameInnerTextOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.InputValueAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Returns <c>input.value</c> for the selected <c>&lt;input&gt;</c> or <c>&lt;textarea&gt;</c>
    /// or <c>&lt;select&gt;</c> element.
    /// </para>
    /// <para>
    /// Throws for non-input elements. However, if the element is inside the <c>&lt;label&gt;</c>
    /// element that has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// returns the value of the control.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<string> InputValueAsync(string selector, FrameInputValueOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.IsCheckedAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Returns whether the element is checked. Throws if the element is not a checkbox
    /// or radio input.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<bool> IsCheckedAsync(string selector, FrameIsCheckedOptions? options = default);

    /// <summary><para>Returns <c>true</c> if the frame has been detached, or <c>false</c> otherwise.</para></summary>
    bool IsDetached { get; }

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.IsDisabledAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>Returns whether the element is disabled, the opposite of <a href="https://playwright.dev/dotnet/docs/actionability#enabled">enabled</a>.</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<bool> IsDisabledAsync(string selector, FrameIsDisabledOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.IsEditableAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>Returns whether the element is <a href="https://playwright.dev/dotnet/docs/actionability#editable">editable</a>.</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<bool> IsEditableAsync(string selector, FrameIsEditableOptions? options = default);

    /// <summary><para>Returns whether the element is <a href="https://playwright.dev/dotnet/docs/actionability#enabled">enabled</a>.</para></summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<bool> IsEnabledAsync(string selector, FrameIsEnabledOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.IsHiddenAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Returns whether the element is hidden, the opposite of <a href="https://playwright.dev/dotnet/docs/actionability#visible">visible</a>.
    /// <see cref="IFrame.IsHiddenAsync"/> that does not match any elements is considered
    /// hidden.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<bool> IsHiddenAsync(string selector, FrameIsHiddenOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.IsVisibleAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Returns whether the element is <a href="https://playwright.dev/dotnet/docs/actionability#visible">visible</a>.
    /// <see cref="IFrame.IsVisibleAsync"/> that does not match any elements is considered
    /// not visible.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<bool> IsVisibleAsync(string selector, FrameIsVisibleOptions? options = default);

    /// <summary>
    /// <para>
    /// The method returns an element locator that can be used to perform actions on this
    /// page / frame. Locator is resolved to the element immediately before performing an
    /// action, so a series of actions on the same locator can in fact be performed on different
    /// DOM elements. That would happen if the DOM structure between those actions has changed.
    /// </para>
    /// <para><a href="https://playwright.dev/dotnet/docs/locators">Learn more about locators</a>.</para>
    /// <para><a href="https://playwright.dev/dotnet/docs/locators">Learn more about locators</a>.</para>
    /// </summary>
    /// <param name="selector">A selector to use when resolving DOM element.</param>
    /// <param name="options">Call options</param>
    ILocator Locator(string selector, FrameLocatorOptions? options = default);

    /// <summary>
    /// <para>Returns frame's name attribute as specified in the tag.</para>
    /// <para>If the name is empty, returns the id attribute instead.</para>
    /// <para>
    /// This value is calculated once when the frame is created, and will not update if
    /// the attribute is changed later.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is calculated once when the frame is created, and will not update if
    /// the attribute is changed later.
    /// </para>
    /// </remarks>
    string Name { get; }

    /// <summary><para>Returns the page containing this frame.</para></summary>
    IPage Page { get; }

    /// <summary><para>Parent frame, if any. Detached frames and main frames return <c>null</c>.</para></summary>
    IFrame? ParentFrame { get; }

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.PressAsync"/> instead. Read more about <a
    /// href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// <see cref="IFrame.PressAsync"/> can specify the intended <a href="https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key">keyboardEvent.key</a>
    /// value or a single character to generate the text for. A superset of the <see cref="IFrame.PressAsync"/>
    /// values can be found <a href="https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values">here</a>.
    /// Examples of the keys are:
    /// </para>
    /// <para>
    /// <c>F1</c> - <c>F12</c>, <c>Digit0</c>- <c>Digit9</c>, <c>KeyA</c>- <c>KeyZ</c>,
    /// <c>Backquote</c>, <c>Minus</c>, <c>Equal</c>, <c>Backslash</c>, <c>Backspace</c>,
    /// <c>Tab</c>, <c>Delete</c>, <c>Escape</c>, <c>ArrowDown</c>, <c>End</c>, <c>Enter</c>,
    /// <c>Home</c>, <c>Insert</c>, <c>PageDown</c>, <c>PageUp</c>, <c>ArrowRight</c>, <c>ArrowUp</c>,
    /// etc.
    /// </para>
    /// <para>
    /// Following modification shortcuts are also supported: <c>Shift</c>, <c>Control</c>,
    /// <c>Alt</c>, <c>Meta</c>, <c>ShiftLeft</c>, <c>ControlOrMeta</c>. <c>ControlOrMeta</c>
    /// resolves to <c>Control</c> on Windows and Linux and to <c>Meta</c> on macOS.
    /// </para>
    /// <para>
    /// Holding down <c>Shift</c> will type the text that corresponds to the <see cref="IFrame.PressAsync"/>
    /// in the upper case.
    /// </para>
    /// <para>
    /// If <see cref="IFrame.PressAsync"/> is a single character, it is case-sensitive,
    /// so the values <c>a</c> and <c>A</c> will generate different respective texts.
    /// </para>
    /// <para>
    /// Shortcuts such as <c>key: "Control+o"</c>, <c>key: "Control++</c> or <c>key: "Control+Shift+T"</c>
    /// are supported as well. When specified with the modifier, modifier is pressed and
    /// being held while the subsequent key is being pressed.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="key">
    /// Name of the key to press or a character to generate, such as <c>ArrowLeft</c> or
    /// <c>a</c>.
    /// </param>
    /// <param name="options">Call options</param>
    Task PressAsync(string selector, string key, FramePressOptions? options = default);

    /// <summary>
    /// <para>Use locator-based <see cref="IFrame.Locator"/> instead. Read more about <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.</para>
    /// <para>Returns the ElementHandle pointing to the frame element.</para>
    /// <para>
    /// The use of <see cref="IElementHandle"/> is discouraged, use <see cref="ILocator"/>
    /// objects and web-first assertions instead.
    /// </para>
    /// <para>
    /// The method finds an element matching the specified selector within the frame. If
    /// no elements match the selector, returns <c>null</c>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The use of <see cref="IElementHandle"/> is discouraged, use <see cref="ILocator"/>
    /// objects and web-first assertions instead.
    /// </para>
    /// </remarks>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="options">Call options</param>
    Task<IElementHandle?> QuerySelectorAsync(string selector, FrameQuerySelectorOptions? options = default);

    /// <summary>
    /// <para>Use locator-based <see cref="IFrame.Locator"/> instead. Read more about <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.</para>
    /// <para>Returns the ElementHandles pointing to the frame elements.</para>
    /// <para>
    /// The use of <see cref="IElementHandle"/> is discouraged, use <see cref="ILocator"/>
    /// objects instead.
    /// </para>
    /// <para>
    /// The method finds all elements matching the specified selector within the frame.
    /// If no elements match the selector, returns empty array.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The use of <see cref="IElementHandle"/> is discouraged, use <see cref="ILocator"/>
    /// objects instead.
    /// </para>
    /// </remarks>
    /// <param name="selector">A selector to query for.</param>
    Task<IReadOnlyList<IElementHandle>> QuerySelectorAllAsync(string selector);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SelectOptionAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method waits for an element matching <see cref="IFrame.SelectOptionAsync"/>,
    /// waits for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks, waits until all specified options are present in the <c>&lt;select&gt;</c>
    /// element and selects these options.
    /// </para>
    /// <para>
    /// If the target element is not a <c>&lt;select&gt;</c> element, this method throws
    /// an error. However, if the element is inside the <c>&lt;label&gt;</c> element that
    /// has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// the control will be used instead.
    /// </para>
    /// <para>Returns the array of option values that have been successfully selected.</para>
    /// <para>
    /// Triggers a <c>change</c> and <c>input</c> event once all the provided options have
    /// been selected.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // Single selection matching the value or label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "blue" });<br/>
    /// // single selection matching both the value and the label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { new SelectOptionValue() { Label = "blue" } });<br/>
    /// // multiple selection<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "red", "green", "blue" });
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="values">
    /// Options to select. If the <c>&lt;select&gt;</c> has the <c>multiple</c> attribute,
    /// all matching options are selected, otherwise only the first option matching one
    /// of the passed options is selected. String values are matching both values and labels.
    /// Option is considered matching if all specified properties match.
    /// </param>
    /// <param name="options">Call options</param>
    Task<IReadOnlyList<string>> SelectOptionAsync(string selector, string values, FrameSelectOptionOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SelectOptionAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method waits for an element matching <see cref="IFrame.SelectOptionAsync"/>,
    /// waits for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks, waits until all specified options are present in the <c>&lt;select&gt;</c>
    /// element and selects these options.
    /// </para>
    /// <para>
    /// If the target element is not a <c>&lt;select&gt;</c> element, this method throws
    /// an error. However, if the element is inside the <c>&lt;label&gt;</c> element that
    /// has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// the control will be used instead.
    /// </para>
    /// <para>Returns the array of option values that have been successfully selected.</para>
    /// <para>
    /// Triggers a <c>change</c> and <c>input</c> event once all the provided options have
    /// been selected.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // Single selection matching the value or label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "blue" });<br/>
    /// // single selection matching both the value and the label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { new SelectOptionValue() { Label = "blue" } });<br/>
    /// // multiple selection<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "red", "green", "blue" });
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="values">
    /// Options to select. If the <c>&lt;select&gt;</c> has the <c>multiple</c> attribute,
    /// all matching options are selected, otherwise only the first option matching one
    /// of the passed options is selected. String values are matching both values and labels.
    /// Option is considered matching if all specified properties match.
    /// </param>
    /// <param name="options">Call options</param>
    Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IElementHandle values, FrameSelectOptionOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SelectOptionAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method waits for an element matching <see cref="IFrame.SelectOptionAsync"/>,
    /// waits for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks, waits until all specified options are present in the <c>&lt;select&gt;</c>
    /// element and selects these options.
    /// </para>
    /// <para>
    /// If the target element is not a <c>&lt;select&gt;</c> element, this method throws
    /// an error. However, if the element is inside the <c>&lt;label&gt;</c> element that
    /// has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// the control will be used instead.
    /// </para>
    /// <para>Returns the array of option values that have been successfully selected.</para>
    /// <para>
    /// Triggers a <c>change</c> and <c>input</c> event once all the provided options have
    /// been selected.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // Single selection matching the value or label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "blue" });<br/>
    /// // single selection matching both the value and the label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { new SelectOptionValue() { Label = "blue" } });<br/>
    /// // multiple selection<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "red", "green", "blue" });
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="values">
    /// Options to select. If the <c>&lt;select&gt;</c> has the <c>multiple</c> attribute,
    /// all matching options are selected, otherwise only the first option matching one
    /// of the passed options is selected. String values are matching both values and labels.
    /// Option is considered matching if all specified properties match.
    /// </param>
    /// <param name="options">Call options</param>
    Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<string> values, FrameSelectOptionOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SelectOptionAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method waits for an element matching <see cref="IFrame.SelectOptionAsync"/>,
    /// waits for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks, waits until all specified options are present in the <c>&lt;select&gt;</c>
    /// element and selects these options.
    /// </para>
    /// <para>
    /// If the target element is not a <c>&lt;select&gt;</c> element, this method throws
    /// an error. However, if the element is inside the <c>&lt;label&gt;</c> element that
    /// has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// the control will be used instead.
    /// </para>
    /// <para>Returns the array of option values that have been successfully selected.</para>
    /// <para>
    /// Triggers a <c>change</c> and <c>input</c> event once all the provided options have
    /// been selected.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // Single selection matching the value or label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "blue" });<br/>
    /// // single selection matching both the value and the label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { new SelectOptionValue() { Label = "blue" } });<br/>
    /// // multiple selection<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "red", "green", "blue" });
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="values">
    /// Options to select. If the <c>&lt;select&gt;</c> has the <c>multiple</c> attribute,
    /// all matching options are selected, otherwise only the first option matching one
    /// of the passed options is selected. String values are matching both values and labels.
    /// Option is considered matching if all specified properties match.
    /// </param>
    /// <param name="options">Call options</param>
    Task<IReadOnlyList<string>> SelectOptionAsync(string selector, SelectOptionValue values, FrameSelectOptionOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SelectOptionAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method waits for an element matching <see cref="IFrame.SelectOptionAsync"/>,
    /// waits for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks, waits until all specified options are present in the <c>&lt;select&gt;</c>
    /// element and selects these options.
    /// </para>
    /// <para>
    /// If the target element is not a <c>&lt;select&gt;</c> element, this method throws
    /// an error. However, if the element is inside the <c>&lt;label&gt;</c> element that
    /// has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// the control will be used instead.
    /// </para>
    /// <para>Returns the array of option values that have been successfully selected.</para>
    /// <para>
    /// Triggers a <c>change</c> and <c>input</c> event once all the provided options have
    /// been selected.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // Single selection matching the value or label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "blue" });<br/>
    /// // single selection matching both the value and the label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { new SelectOptionValue() { Label = "blue" } });<br/>
    /// // multiple selection<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "red", "green", "blue" });
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="values">
    /// Options to select. If the <c>&lt;select&gt;</c> has the <c>multiple</c> attribute,
    /// all matching options are selected, otherwise only the first option matching one
    /// of the passed options is selected. String values are matching both values and labels.
    /// Option is considered matching if all specified properties match.
    /// </param>
    /// <param name="options">Call options</param>
    Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<IElementHandle> values, FrameSelectOptionOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SelectOptionAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method waits for an element matching <see cref="IFrame.SelectOptionAsync"/>,
    /// waits for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks, waits until all specified options are present in the <c>&lt;select&gt;</c>
    /// element and selects these options.
    /// </para>
    /// <para>
    /// If the target element is not a <c>&lt;select&gt;</c> element, this method throws
    /// an error. However, if the element is inside the <c>&lt;label&gt;</c> element that
    /// has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// the control will be used instead.
    /// </para>
    /// <para>Returns the array of option values that have been successfully selected.</para>
    /// <para>
    /// Triggers a <c>change</c> and <c>input</c> event once all the provided options have
    /// been selected.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// // Single selection matching the value or label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "blue" });<br/>
    /// // single selection matching both the value and the label<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { new SelectOptionValue() { Label = "blue" } });<br/>
    /// // multiple selection<br/>
    /// await frame.SelectOptionAsync("select#colors", new[] { "red", "green", "blue" });
    /// </code>
    /// </summary>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="values">
    /// Options to select. If the <c>&lt;select&gt;</c> has the <c>multiple</c> attribute,
    /// all matching options are selected, otherwise only the first option matching one
    /// of the passed options is selected. String values are matching both values and labels.
    /// Option is considered matching if all specified properties match.
    /// </param>
    /// <param name="options">Call options</param>
    Task<IReadOnlyList<string>> SelectOptionAsync(string selector, IEnumerable<SelectOptionValue> values, FrameSelectOptionOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SetCheckedAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method checks or unchecks an element matching <see cref="IFrame.SetCheckedAsync"/>
    /// by performing the following steps:
    /// </para>
    /// <list type="ordinal">
    /// <item><description>
    /// Find an element matching <see cref="IFrame.SetCheckedAsync"/>. If there is none,
    /// wait until a matching element is attached to the DOM.
    /// </description></item>
    /// <item><description>
    /// Ensure that matched element is a checkbox or a radio input. If not, this method
    /// throws.
    /// </description></item>
    /// <item><description>If the element already has the right checked state, this method returns immediately.</description></item>
    /// <item><description>
    /// Wait for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks on the matched element, unless <see cref="IFrame.SetCheckedAsync"/> option
    /// is set. If the element is detached during the checks, the whole action is retried.
    /// </description></item>
    /// <item><description>Scroll the element into view if needed.</description></item>
    /// <item><description>Use <see cref="IPage.Mouse"/> to click in the center of the element.</description></item>
    /// <item><description>Ensure that the element is now checked or unchecked. If not, this method throws.</description></item>
    /// </list>
    /// <para>
    /// When all steps combined have not finished during the specified <see cref="IFrame.SetCheckedAsync"/>,
    /// this method throws a <see cref="TimeoutException"/>. Passing zero timeout disables
    /// this.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="checkedState">Whether to check or uncheck the checkbox.</param>
    /// <param name="options">Call options</param>
    Task SetCheckedAsync(string selector, bool checkedState, FrameSetCheckedOptions? options = default);

    /// <summary>
    /// <para>
    /// This method internally calls <a href="https://developer.mozilla.org/en-US/docs/Web/API/Document/write">document.write()</a>,
    /// inheriting all its specific characteristics and behaviors.
    /// </para>
    /// </summary>
    /// <param name="html">HTML markup to assign to the page.</param>
    /// <param name="options">Call options</param>
    Task SetContentAsync(string html, FrameSetContentOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SetInputFilesAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Sets the value of the file input to these file paths or files. If some of the <c>filePaths</c>
    /// are relative paths, then they are resolved relative to the current working directory.
    /// For empty array, clears the selected files.
    /// </para>
    /// <para>
    /// This method expects <see cref="IFrame.SetInputFilesAsync"/> to point to an <a href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input">input
    /// element</a>. However, if the element is inside the <c>&lt;label&gt;</c> element
    /// that has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// targets the control instead.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="files">
    /// </param>
    /// <param name="options">Call options</param>
    Task SetInputFilesAsync(string selector, string files, FrameSetInputFilesOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SetInputFilesAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Sets the value of the file input to these file paths or files. If some of the <c>filePaths</c>
    /// are relative paths, then they are resolved relative to the current working directory.
    /// For empty array, clears the selected files.
    /// </para>
    /// <para>
    /// This method expects <see cref="IFrame.SetInputFilesAsync"/> to point to an <a href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input">input
    /// element</a>. However, if the element is inside the <c>&lt;label&gt;</c> element
    /// that has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// targets the control instead.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="files">
    /// </param>
    /// <param name="options">Call options</param>
    Task SetInputFilesAsync(string selector, IEnumerable<string> files, FrameSetInputFilesOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SetInputFilesAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Sets the value of the file input to these file paths or files. If some of the <c>filePaths</c>
    /// are relative paths, then they are resolved relative to the current working directory.
    /// For empty array, clears the selected files.
    /// </para>
    /// <para>
    /// This method expects <see cref="IFrame.SetInputFilesAsync"/> to point to an <a href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input">input
    /// element</a>. However, if the element is inside the <c>&lt;label&gt;</c> element
    /// that has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// targets the control instead.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="files">
    /// </param>
    /// <param name="options">Call options</param>
    Task SetInputFilesAsync(string selector, FilePayload files, FrameSetInputFilesOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.SetInputFilesAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Sets the value of the file input to these file paths or files. If some of the <c>filePaths</c>
    /// are relative paths, then they are resolved relative to the current working directory.
    /// For empty array, clears the selected files.
    /// </para>
    /// <para>
    /// This method expects <see cref="IFrame.SetInputFilesAsync"/> to point to an <a href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input">input
    /// element</a>. However, if the element is inside the <c>&lt;label&gt;</c> element
    /// that has an associated <a href="https://developer.mozilla.org/en-US/docs/Web/API/HTMLLabelElement/control">control</a>,
    /// targets the control instead.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="files">
    /// </param>
    /// <param name="options">Call options</param>
    Task SetInputFilesAsync(string selector, IEnumerable<FilePayload> files, FrameSetInputFilesOptions? options = default);

    /// <summary>
    /// <para>Use locator-based <see cref="ILocator.TapAsync"/> instead. Read more about <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.</para>
    /// <para>
    /// This method taps an element matching <see cref="IFrame.TapAsync"/> by performing
    /// the following steps:
    /// </para>
    /// <list type="ordinal">
    /// <item><description>
    /// Find an element matching <see cref="IFrame.TapAsync"/>. If there is none, wait until
    /// a matching element is attached to the DOM.
    /// </description></item>
    /// <item><description>
    /// Wait for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks on the matched element, unless <see cref="IFrame.TapAsync"/> option is set.
    /// If the element is detached during the checks, the whole action is retried.
    /// </description></item>
    /// <item><description>Scroll the element into view if needed.</description></item>
    /// <item><description>
    /// Use <see cref="IPage.Touchscreen"/> to tap the center of the element, or the specified
    /// <see cref="IFrame.TapAsync"/>.
    /// </description></item>
    /// </list>
    /// <para>
    /// When all steps combined have not finished during the specified <see cref="IFrame.TapAsync"/>,
    /// this method throws a <see cref="TimeoutException"/>. Passing zero timeout disables
    /// this.
    /// </para>
    /// <para>
    /// <c>frame.tap()</c> requires that the <c>hasTouch</c> option of the browser context
    /// be set to true.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>frame.tap()</c> requires that the <c>hasTouch</c> option of the browser context
    /// be set to true.
    /// </para>
    /// </remarks>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task TapAsync(string selector, FrameTapOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.TextContentAsync"/> instead. Read more about
    /// <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>Returns <c>element.textContent</c>.</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task<string?> TextContentAsync(string selector, FrameTextContentOptions? options = default);

    /// <summary><para>Returns the page title.</para></summary>
    Task<string> TitleAsync();

    /// <summary>
    /// <para>
    /// **DEPRECATED** In most cases, you should use <see cref="ILocator.FillAsync"/> instead.
    /// You only need to press keys one by one if there is special keyboard handling on
    /// the page - in this case use <see cref="ILocator.PressSequentiallyAsync"/>.
    /// </para>
    /// <para>
    /// Sends a <c>keydown</c>, <c>keypress</c>/<c>input</c>, and <c>keyup</c> event for
    /// each character in the text. <c>frame.type</c> can be used to send fine-grained keyboard
    /// events. To fill values in form fields, use <see cref="IFrame.FillAsync"/>.
    /// </para>
    /// <para>To press a special key, like <c>Control</c> or <c>ArrowDown</c>, use <see cref="IKeyboard.PressAsync"/>.</para>
    /// <para>**Usage**</para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="text">A text to type into a focused element.</param>
    /// <param name="options">Call options</param>
    [System.Obsolete]
    Task TypeAsync(string selector, string text, FrameTypeOptions? options = default);

    /// <summary>
    /// <para>
    /// Use locator-based <see cref="ILocator.UncheckAsync"/> instead. Read more about <a
    /// href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// This method checks an element matching <see cref="IFrame.UncheckAsync"/> by performing
    /// the following steps:
    /// </para>
    /// <list type="ordinal">
    /// <item><description>
    /// Find an element matching <see cref="IFrame.UncheckAsync"/>. If there is none, wait
    /// until a matching element is attached to the DOM.
    /// </description></item>
    /// <item><description>
    /// Ensure that matched element is a checkbox or a radio input. If not, this method
    /// throws. If the element is already unchecked, this method returns immediately.
    /// </description></item>
    /// <item><description>
    /// Wait for <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks on the matched element, unless <see cref="IFrame.UncheckAsync"/> option is
    /// set. If the element is detached during the checks, the whole action is retried.
    /// </description></item>
    /// <item><description>Scroll the element into view if needed.</description></item>
    /// <item><description>Use <see cref="IPage.Mouse"/> to click in the center of the element.</description></item>
    /// <item><description>Ensure that the element is now unchecked. If not, this method throws.</description></item>
    /// </list>
    /// <para>
    /// When all steps combined have not finished during the specified <see cref="IFrame.UncheckAsync"/>,
    /// this method throws a <see cref="TimeoutException"/>. Passing zero timeout disables
    /// this.
    /// </para>
    /// </summary>
    /// <param name="selector">
    /// A selector to search for an element. If there are multiple elements satisfying the
    /// selector, the first will be used.
    /// </param>
    /// <param name="options">Call options</param>
    Task UncheckAsync(string selector, FrameUncheckOptions? options = default);

    /// <summary><para>Returns frame's url.</para></summary>
    string Url { get; }

    /// <summary>
    /// <para>
    /// Returns when the <see cref="IFrame.WaitForFunctionAsync"/> returns a truthy value,
    /// returns that value.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>
    /// The <see cref="IFrame.WaitForFunctionAsync"/> can be used to observe viewport size
    /// change:
    /// </para>
    /// <code>
    /// using Microsoft.Playwright;<br/>
    /// using System.Threading.Tasks;<br/>
    /// <br/>
    /// class FrameExamples<br/>
    /// {<br/>
    ///     public static async Task Main()<br/>
    ///     {<br/>
    ///         using var playwright = await Playwright.CreateAsync();<br/>
    ///         await using var browser = await playwright.Firefox.LaunchAsync();<br/>
    ///         var page = await browser.NewPageAsync();<br/>
    ///         await page.SetViewportSizeAsync(50, 50);<br/>
    ///         await page.MainFrame.WaitForFunctionAsync("window.innerWidth &lt; 100");<br/>
    ///     }<br/>
    /// }
    /// </code>
    /// <para>To pass an argument to the predicate of <c>frame.waitForFunction</c> function:</para>
    /// <code>
    /// var selector = ".foo";<br/>
    /// await page.MainFrame.WaitForFunctionAsync("selector =&gt; !!document.querySelector(selector)", selector);
    /// </code>
    /// </summary>
    /// <param name="expression">
    /// JavaScript expression to be evaluated in the browser context. If the expression
    /// evaluates to a function, the function is automatically invoked.
    /// </param>
    /// <param name="arg">Optional argument to pass to <see cref="IFrame.WaitForFunctionAsync"/>.</param>
    /// <param name="options">Call options</param>
    Task<IJSHandle> WaitForFunctionAsync(string expression, object? arg = default, FrameWaitForFunctionOptions? options = default);

    /// <summary>
    /// <para>Waits for the required load state to be reached.</para>
    /// <para>
    /// This returns when the frame reaches a required load state, <c>load</c> by default.
    /// The navigation must have been committed when this method is called. If current document
    /// has already reached the required state, resolves immediately.
    /// </para>
    /// <para>
    /// Most of the time, this method is not needed because Playwright <a href="https://playwright.dev/dotnet/docs/actionability">auto-waits
    /// before every action</a>.
    /// </para>
    /// <para>**Usage**</para>
    /// <code>
    /// await frame.ClickAsync("button");<br/>
    /// await frame.WaitForLoadStateAsync(); // Defaults to LoadState.Load
    /// </code>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most of the time, this method is not needed because Playwright <a href="https://playwright.dev/dotnet/docs/actionability">auto-waits
    /// before every action</a>.
    /// </para>
    /// </remarks>
    /// <param name="state">
    /// Optional load state to wait for, defaults to <c>load</c>. If the state has been
    /// already reached while loading current document, the method resolves immediately.
    /// Can be one of:
    /// <list type="bullet">
    /// <item><description><c>'load'</c> - wait for the <c>load</c> event to be fired.</description></item>
    /// <item><description><c>'domcontentloaded'</c> - wait for the <c>DOMContentLoaded</c> event to be fired.</description></item>
    /// <item><description>
    /// <c>'networkidle'</c> - **DISCOURAGED** wait until there are no network connections
    /// for at least <c>500</c> ms. Don't use this method for testing, rely on web assertions
    /// to assess readiness instead.
    /// </description></item>
    /// </list>
    /// </param>
    /// <param name="options">Call options</param>
    Task WaitForLoadStateAsync(LoadState? state = default, FrameWaitForLoadStateOptions? options = default);

    /// <summary>
    /// <para>
    /// **DEPRECATED** This method is inherently racy, please use <see cref="IFrame.WaitForURLAsync"/>
    /// instead.
    /// </para>
    /// <para>
    /// Waits for the frame navigation and returns the main resource response. In case of
    /// multiple redirects, the navigation will resolve with the response of the last redirect.
    /// In case of navigation to a different anchor or navigation due to History API usage,
    /// the navigation will resolve with <c>null</c>.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>
    /// This method waits for the frame to navigate to a new URL. It is useful for when
    /// you run code which will indirectly cause the frame to navigate. Consider this example:
    /// </para>
    /// <code>
    /// await frame.RunAndWaitForNavigationAsync(async () =&gt;<br/>
    /// {<br/>
    ///     // Clicking the link will indirectly cause a navigation.<br/>
    ///     await frame.ClickAsync("a.delayed-navigation");<br/>
    /// });<br/>
    /// <br/>
    /// // Resolves after navigation has finished
    /// </code>
    /// <para>
    /// Usage of the <a href="https://developer.mozilla.org/en-US/docs/Web/API/History_API">History
    /// API</a> to change the URL is considered a navigation.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage of the <a href="https://developer.mozilla.org/en-US/docs/Web/API/History_API">History
    /// API</a> to change the URL is considered a navigation.
    /// </para>
    /// </remarks>
    /// <param name="options">Call options</param>
    [System.Obsolete]
    Task<IResponse?> WaitForNavigationAsync(FrameWaitForNavigationOptions? options = default);

    /// <summary>
    /// <para>
    /// **DEPRECATED** This method is inherently racy, please use <see cref="IFrame.WaitForURLAsync"/>
    /// instead.
    /// </para>
    /// <para>
    /// Waits for the frame navigation and returns the main resource response. In case of
    /// multiple redirects, the navigation will resolve with the response of the last redirect.
    /// In case of navigation to a different anchor or navigation due to History API usage,
    /// the navigation will resolve with <c>null</c>.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>
    /// This method waits for the frame to navigate to a new URL. It is useful for when
    /// you run code which will indirectly cause the frame to navigate. Consider this example:
    /// </para>
    /// <code>
    /// await frame.RunAndWaitForNavigationAsync(async () =&gt;<br/>
    /// {<br/>
    ///     // Clicking the link will indirectly cause a navigation.<br/>
    ///     await frame.ClickAsync("a.delayed-navigation");<br/>
    /// });<br/>
    /// <br/>
    /// // Resolves after navigation has finished
    /// </code>
    /// <para>
    /// Usage of the <a href="https://developer.mozilla.org/en-US/docs/Web/API/History_API">History
    /// API</a> to change the URL is considered a navigation.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usage of the <a href="https://developer.mozilla.org/en-US/docs/Web/API/History_API">History
    /// API</a> to change the URL is considered a navigation.
    /// </para>
    /// </remarks>
    /// <param name="action">Action that triggers the event.</param>
    /// <param name="options">Call options</param>
    [System.Obsolete]
    Task<IResponse?> RunAndWaitForNavigationAsync(Func<Task> action, FrameRunAndWaitForNavigationOptions? options = default);

    /// <summary>
    /// <para>
    /// Use web assertions that assert visibility or a locator-based <see cref="ILocator.WaitForAsync"/>
    /// instead. Read more about <a href="https://playwright.dev/dotnet/docs/locators">locators</a>.
    /// </para>
    /// <para>
    /// Returns when element specified by selector satisfies <see cref="IFrame.WaitForSelectorAsync"/>
    /// option. Returns <c>null</c> if waiting for <c>hidden</c> or <c>detached</c>.
    /// </para>
    /// <para>
    /// Playwright automatically waits for element to be ready before performing an action.
    /// Using <see cref="ILocator"/> objects and web-first assertions make the code wait-for-selector-free.
    /// </para>
    /// <para>
    /// Wait for the <see cref="IFrame.WaitForSelectorAsync"/> to satisfy <see cref="IFrame.WaitForSelectorAsync"/>
    /// option (either appear/disappear from dom, or become visible/hidden). If at the moment
    /// of calling the method <see cref="IFrame.WaitForSelectorAsync"/> already satisfies
    /// the condition, the method will return immediately. If the selector doesn't satisfy
    /// the condition for the <see cref="IFrame.WaitForSelectorAsync"/> milliseconds, the
    /// function will throw.
    /// </para>
    /// <para>**Usage**</para>
    /// <para>This method works across navigations:</para>
    /// <code>
    /// using Microsoft.Playwright;<br/>
    /// using System;<br/>
    /// using System.Threading.Tasks;<br/>
    /// <br/>
    /// class FrameExamples<br/>
    /// {<br/>
    ///     public static async Task Main()<br/>
    ///     {<br/>
    ///         using var playwright = await Playwright.CreateAsync();<br/>
    ///         await using var browser = await playwright.Chromium.LaunchAsync();<br/>
    ///         var page = await browser.NewPageAsync();<br/>
    /// <br/>
    ///         foreach (var currentUrl in new[] { "https://www.google.com", "https://bbc.com" })<br/>
    ///         {<br/>
    ///             await page.GotoAsync(currentUrl);<br/>
    ///             element = await page.MainFrame.WaitForSelectorAsync("img");<br/>
    ///             Console.WriteLine($"Loaded image: {await element.GetAttributeAsync("src")}");<br/>
    ///         }<br/>
    ///     }<br/>
    /// }
    /// </code>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Playwright automatically waits for element to be ready before performing an action.
    /// Using <see cref="ILocator"/> objects and web-first assertions make the code wait-for-selector-free.
    ///
    /// </para>
    /// </remarks>
    /// <param name="selector">A selector to query for.</param>
    /// <param name="options">Call options</param>
    Task<IElementHandle?> WaitForSelectorAsync(string selector, FrameWaitForSelectorOptions? options = default);

    /// <summary>
    /// <para>
    /// Never wait for timeout in production. Tests that wait for time are inherently flaky.
    /// Use <see cref="ILocator"/> actions and web assertions that wait automatically.
    /// </para>
    /// <para>Waits for the given <see cref="IFrame.WaitForTimeoutAsync"/> in milliseconds.</para>
    /// <para>
    /// Note that <c>frame.waitForTimeout()</c> should only be used for debugging. Tests
    /// using the timer in production are going to be flaky. Use signals such as network
    /// events, selectors becoming visible and others instead.
    /// </para>
    /// </summary>
    /// <param name="timeout">A timeout to wait for</param>
    Task WaitForTimeoutAsync(float timeout);

    /// <summary>
    /// <para>Waits for the frame to navigate to the given URL.</para>
    /// <para>**Usage**</para>
    /// <code>
    /// await frame.ClickAsync("a.delayed-navigation"); // clicking the link will indirectly cause a navigation<br/>
    /// await frame.WaitForURLAsync("**/target.html");
    /// </code>
    /// </summary>
    /// <param name="url">
    /// A glob pattern, regex pattern or predicate receiving <see cref="URL"/> to match
    /// while waiting for the navigation. Note that if the parameter is a string without
    /// wildcard characters, the method will wait for navigation to URL that is exactly
    /// equal to the string.
    /// </param>
    /// <param name="options">Call options</param>
    Task WaitForURLAsync(string url, FrameWaitForURLOptions? options = default);

    /// <summary>
    /// <para>Waits for the frame to navigate to the given URL.</para>
    /// <para>**Usage**</para>
    /// <code>
    /// await frame.ClickAsync("a.delayed-navigation"); // clicking the link will indirectly cause a navigation<br/>
    /// await frame.WaitForURLAsync("**/target.html");
    /// </code>
    /// </summary>
    /// <param name="url">
    /// A glob pattern, regex pattern or predicate receiving <see cref="URL"/> to match
    /// while waiting for the navigation. Note that if the parameter is a string without
    /// wildcard characters, the method will wait for navigation to URL that is exactly
    /// equal to the string.
    /// </param>
    /// <param name="options">Call options</param>
    Task WaitForURLAsync(Regex url, FrameWaitForURLOptions? options = default);

    /// <summary>
    /// <para>Waits for the frame to navigate to the given URL.</para>
    /// <para>**Usage**</para>
    /// <code>
    /// await frame.ClickAsync("a.delayed-navigation"); // clicking the link will indirectly cause a navigation<br/>
    /// await frame.WaitForURLAsync("**/target.html");
    /// </code>
    /// </summary>
    /// <param name="url">
    /// A glob pattern, regex pattern or predicate receiving <see cref="URL"/> to match
    /// while waiting for the navigation. Note that if the parameter is a string without
    /// wildcard characters, the method will wait for navigation to URL that is exactly
    /// equal to the string.
    /// </param>
    /// <param name="options">Call options</param>
    Task WaitForURLAsync(Func<string, bool> url, FrameWaitForURLOptions? options = default);
}
