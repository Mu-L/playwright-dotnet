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

public class MouseClickOptions
{
    public MouseClickOptions() { }

    public MouseClickOptions(MouseClickOptions clone)
    {
        if (clone == null)
        {
            return;
        }

        Button = clone.Button;
        ClickCount = clone.ClickCount;
        Delay = clone.Delay;
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
}
