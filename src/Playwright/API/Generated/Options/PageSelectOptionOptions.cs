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

using System.Text.Json.Serialization;

namespace Microsoft.Playwright;

public class PageSelectOptionOptions
{
    public PageSelectOptionOptions() { }

    public PageSelectOptionOptions(PageSelectOptionOptions clone)
    {
        if (clone == null)
        {
            return;
        }

        Force = clone.Force;
        NoWaitAfter = clone.NoWaitAfter;
        Strict = clone.Strict;
        Timeout = clone.Timeout;
    }

    /// <summary>
    /// <para>
    /// Whether to bypass the <a href="https://playwright.dev/dotnet/docs/actionability">actionability</a>
    /// checks. Defaults to <c>false</c>.
    /// </para>
    /// </summary>
    [JsonPropertyName("force")]
    public bool? Force { get; set; }

    /// <summary>
    /// <para>**DEPRECATED** This option has no effect.</para>
    /// <para>This option has no effect.</para>
    /// </summary>
    [JsonPropertyName("noWaitAfter")]
    [System.Obsolete]
    public bool? NoWaitAfter { get; set; }

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
}
