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

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Playwright;

public class FrameClickOptions
{
    public FrameClickOptions() { }

    public FrameClickOptions(FrameClickOptions clone)
    {
        if (clone == null)
        {
            return;
        }

        Button = clone.Button;
        ClickCount = clone.ClickCount;
        Delay = clone.Delay;
        Force = clone.Force;
        Modifiers = clone.Modifiers;
        NoWaitAfter = clone.NoWaitAfter;
        Position = clone.Position;
        Strict = clone.Strict;
        Timeout = clone.Timeout;
        Trial = clone.Trial;
    }

    /// <summary><para>Defaults to <c>left</c>.</para></summary>
    [JsonPropertyName("button")]
    public MouseButton? Button { get; set; }

    /// <summary><para>defaults to 1. See <see cref="UIEvent.detail"/>.</para></summary>
    [JsonPropertyName("clickCount")]
    public int? ClickCount { get; set; }

    /// <summary>
    /// <para>
    /// Time to wait between <c>mousedown</c> and <c>mouseup</c> in milliseconds. Defaults
    /// to 0.
    /// </para>
    /// </summary>
    [JsonPropertyName("delay")]
    public float? Delay { get; set; }

    /// <summary>
    /// <para>
    /// Whether to bypass the <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks. Defaults to <c>false</c>.
    /// </para>
    /// </summary>
    [JsonPropertyName("force")]
    public bool? Force { get; set; }

    /// <summary>
    /// <para>
    /// Modifier keys to press. Ensures that only these modifiers are pressed during the
    /// operation, and then restores current modifiers back. If not specified, currently
    /// pressed modifiers are used. "ControlOrMeta" resolves to "Control" on Windows and
    /// Linux and to "Meta" on macOS.
    /// </para>
    /// </summary>
    [JsonPropertyName("modifiers")]
    public IEnumerable<KeyboardModifier>? Modifiers { get; set; }

    /// <summary>
    /// <para>**DEPRECATED** This option will default to <c>true</c> in the future.</para>
    /// <para>
    /// Actions that initiate navigations are waiting for these navigations to happen and
    /// for pages to start loading. You can opt out of waiting via setting this flag. You
    /// would only need this option in the exceptional cases such as navigating to inaccessible
    /// pages. Defaults to <c>false</c>.
    /// </para>
    /// </summary>
    [JsonPropertyName("noWaitAfter")]
    [System.Obsolete]
    public bool? NoWaitAfter { get; set; }

    /// <summary>
    /// <para>
    /// A point to use relative to the top-left corner of element padding box. If not specified,
    /// uses some visible point of the element.
    /// </para>
    /// </summary>
    [JsonPropertyName("position")]
    public Position? Position { get; set; }

    /// <summary>
    /// <para>
    /// When true, the call requires selector to resolve to a single element. If given selector
    /// resolves to more than one element, the call throws an exception.
    /// </para>
    /// </summary>
    [JsonPropertyName("strict")]
    public bool? Strict { get; set; }

    /// <summary>
    /// <para>
    /// Maximum time in milliseconds. Defaults to <c>30000</c> (30 seconds). Pass <c>0</c>
    /// to disable timeout. The default value can be changed by using the <see cref="IBrowserContext.SetDefaultTimeout"/>
    /// or <see cref="IPage.SetDefaultTimeout"/> methods.
    /// </para>
    /// </summary>
    [JsonPropertyName("timeout")]
    public float? Timeout { get; set; }

    /// <summary>
    /// <para>
    /// When set, this method only performs the <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks and skips the action. Defaults to <c>false</c>. Useful to wait until the
    /// element is ready for the action without performing it. Note that keyboard <c>modifiers</c>
    /// will be pressed regardless of <c>trial</c> to allow testing elements which are only
    /// visible when those keys are pressed.
    /// </para>
    /// </summary>
    [JsonPropertyName("trial")]
    public bool? Trial { get; set; }
}
