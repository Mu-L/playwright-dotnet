/*
 * MIT License
 *
 * Copyright (c) 2020 Darío Kondratiuk
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

namespace Microsoft.Playwright.Tests;

///<playwright-file>pdf.spec.ts</playwright-file>
public class PdfTests : PageTestEx
{
    [PlaywrightTest("pdf.spec.ts", "should be able to save file")]
    [Skip(SkipAttribute.Targets.Firefox, SkipAttribute.Targets.Webkit)]
    public async Task ShouldBeAbleToSaveFile()
    {
        var baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "workspace");
        string outputFile = Path.Combine(baseDirectory, "output.pdf");
        var fileInfo = new FileInfo(outputFile);
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
        }
        await Page.PdfAsync(new() { Path = outputFile, Format = PaperFormat.Letter });
        fileInfo = new(outputFile);
        Assert.True(new FileInfo(outputFile).Length > 0);
        if (fileInfo.Exists)
        {
            fileInfo.Delete();
        }
    }

    [PlaywrightTest("pdf.spec.ts", "should be able to save file without passing options")]
    [Skip(SkipAttribute.Targets.Firefox, SkipAttribute.Targets.Webkit)]
    public async Task ShouldBeAbleToSaveFileWithoutPassingOptions()
    {
        await Page.GotoAsync(Server.EmptyPage);
        var result = await Page.PdfAsync();
        Assert.True(result.Length > 0);
    }

    [PlaywrightTest("pdf.spec.ts", "should be able to generate outline")]
    [Skip(SkipAttribute.Targets.Firefox, SkipAttribute.Targets.Webkit)]
    public async Task ShouldBeAbleToGenerateOutline()
    {
        await Page.GotoAsync(Server.Prefix + "/headings.html");
        var outputFileNoOutline = await Page.PdfAsync();
        var outputFileOutline = await Page.PdfAsync(new() { Tagged = true, Outline = true });
        Assert.Greater(outputFileOutline.Length, outputFileNoOutline.Length);
    }
}
