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

public class APIRequestContextStorageStateOptions
{
    public APIRequestContextStorageStateOptions() { }

    public APIRequestContextStorageStateOptions(APIRequestContextStorageStateOptions clone)
    {
        if (clone == null)
        {
            return;
        }

        IndexedDB = clone.IndexedDB;
        Path = clone.Path;
    }

    /// <summary><para>Set to <c>true</c> to include IndexedDB in the storage state snapshot.</para></summary>
    [JsonPropertyName("indexedDB")]
    public bool? IndexedDB { get; set; }

    /// <summary>
    /// <para>
    /// The file path to save the storage state to. If <see cref="IAPIRequestContext.StorageStateAsync"/>
    /// is a relative path, then it is resolved relative to current working directory. If
    /// no path is provided, storage state is still returned, but won't be saved to the
    /// disk.
    /// </para>
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }
}
