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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright.Transport.Protocol;

namespace Microsoft.Playwright.Core;

internal class LocatorAssertions : AssertionsBase, ILocatorAssertions
{
    public LocatorAssertions(ILocator locator, bool isNot) : base(isNot)
    {
        ActualLocator = (Locator)locator;
    }

    private Locator ActualLocator { get; }

    public ILocatorAssertions Not => new LocatorAssertions(ActualLocator, !IsNot);

    protected override Task<FrameExpectResult> CallExpectAsync(string expression, FrameExpectOptions expectOptions, string title)
    {
        return ActualLocator.ExpectAsync(expression, expectOptions, title);
    }

    public Task ToBeAttachedAsync(LocatorAssertionsToBeAttachedOptions? options = null)
    {
        var attached = options == null || options.Attached == null || options.Attached == true;
        return ExpectTrueAsync(attached ? "to.be.attached" : "to.be.detached", $"Locator expected {(!attached ? "not " : string.Empty)}to be attached", "Expect \"ToBeAttachedAsync\"", ConvertToFrameExpectOptions(options));
    }

    public Task ToBeCheckedAsync(LocatorAssertionsToBeCheckedOptions? options = null)
    {
        var expectedValue = new Dictionary<string, object>();
        if (options?.Checked != null)
        {
            expectedValue["checked"] = options.Checked;
        }
        if (options?.Indeterminate != null)
        {
            expectedValue["indeterminate"] = options.Indeterminate;
        }
        var frameExpectedOptions = ConvertToFrameExpectOptions(options) ?? new();
        frameExpectedOptions.ExpectedValue = ScriptsHelper.SerializedArgument(expectedValue);
        var checkedString = options?.Indeterminate == true ? "indeterminate" : "checked";
        return ExpectTrueAsync("to.be.checked", $"Locator expected {(options?.Checked == false ? "not " : string.Empty)}to be {checkedString}", "Expect \"ToBeCheckedAsync\"", frameExpectedOptions);
    }

    public Task ToBeDisabledAsync(LocatorAssertionsToBeDisabledOptions? options = null) => ExpectTrueAsync("to.be.disabled", "Locator expected to be disabled", "Expect \"ToBeDisabledAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToBeEditableAsync(LocatorAssertionsToBeEditableOptions? options = null)
    {
        var editable = options == null || options.Editable == null || options.Editable == true;
        var editableString = editable ? "editable" : "read-only";
        return ExpectTrueAsync(editable ? "to.be.editable" : "to.be.readonly", $"Locator expected to be {editableString}", "Expect \"ToBeEditableAsync\"", ConvertToFrameExpectOptions(options));
    }

    public Task ToBeEmptyAsync(LocatorAssertionsToBeEmptyOptions? options = null) => ExpectTrueAsync("to.be.empty", "Locator expected to be empty", "Expect \"ToBeEmptyAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToBeEnabledAsync(LocatorAssertionsToBeEnabledOptions? options = null)
    {
        var enabled = options == null || options.Enabled == null || options.Enabled == true;
        var enabledString = enabled ? "enabled" : "disabled";
        return ExpectTrueAsync(enabled ? "to.be.enabled" : "to.be.disabled", $"Locator expected to be {enabledString}", "Expect \"ToBeEnabledAsync\"", ConvertToFrameExpectOptions(options));
    }

    public Task ToBeFocusedAsync(LocatorAssertionsToBeFocusedOptions? options = null) => ExpectTrueAsync("to.be.focused", "Locator expected to be focused", "Expect \"ToBeFocusedAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToBeHiddenAsync(LocatorAssertionsToBeHiddenOptions? options = null) => ExpectTrueAsync("to.be.hidden", "Locator expected to be hidden", "Expect \"ToBeHiddenAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToBeInViewportAsync(LocatorAssertionsToBeInViewportOptions? options = null)
    {
        var frameExpectOptions = ConvertToFrameExpectOptions(options);
        if (options?.Ratio != null)
        {
            frameExpectOptions.ExpectedNumber = (float)options.Ratio;
        }
        return ExpectTrueAsync("to.be.in.viewport", "Locator expected to be in viewport", "Expect \"ToBeInViewportAsync\"", frameExpectOptions);
    }

    public Task ToBeVisibleAsync(LocatorAssertionsToBeVisibleOptions? options = null)
    {
        var visible = options == null || options.Visible == null || options.Visible == true;
        var visibleString = visible ? "visible" : "hidden";
        return ExpectTrueAsync(visible ? "to.be.visible" : "to.be.hidden", $"Locator expected to be {visibleString}", "Expect \"ToBeVisibleAsync\"", ConvertToFrameExpectOptions(options));
    }

    private Task ExpectTrueAsync(string expression, string message, string title, FrameExpectOptions options)
    {
        return ExpectImplAsync(expression, null as ExpectedTextValue[], null, message, title, options);
    }

    public Task ToContainTextAsync(string expected, LocatorAssertionsToContainTextOptions? options = null) =>
        ExpectImplAsync("to.have.text", new ExpectedTextValue() { String = expected, MatchSubstring = true, NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false }, expected, "Locator expected to contain text", "Expect \"ToContainTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToContainTextAsync(Regex expected, LocatorAssertionsToContainTextOptions? options = null) =>
        ExpectImplAsync("to.have.text", ExpectedRegex(expected, new() { MatchSubstring = true, NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false }), expected, "Locator expected text matching regex", "Expect \"ToContainTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToContainTextAsync(IEnumerable<string> expected, LocatorAssertionsToContainTextOptions? options = null) =>
        ExpectImplAsync("to.contain.text.array", expected.Select(text => new ExpectedTextValue() { String = text, MatchSubstring = true, NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false }).ToArray(), expected, "Locator expected to contain text", "Expect \"ToContainTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToContainTextAsync(IEnumerable<Regex> expected, LocatorAssertionsToContainTextOptions? options = null) =>
        ExpectImplAsync("to.contain.text.array", expected.Select(regex => ExpectedRegex(regex, new() { MatchSubstring = true, NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false })).ToArray(), expected, "Locator expected text matching regex", "Expect \"ToContainTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveAttributeAsync(string name, string value, LocatorAssertionsToHaveAttributeOptions? options = null) =>
        ToHaveAttributeAsync(name, new() { String = value, IgnoreCase = options?.IgnoreCase }, value, options);

    public Task ToHaveAttributeAsync(string name, Regex value, LocatorAssertionsToHaveAttributeOptions? options = null) =>
        ToHaveAttributeAsync(name, ExpectedRegex(value), value, options);

    private Task ToHaveAttributeAsync(string name, ExpectedTextValue expectedText, object expectedValue, LocatorAssertionsToHaveAttributeOptions? options = null)
    {
        var commonOptions = ConvertToFrameExpectOptions(options);
        commonOptions.ExpressionArg = name;
        string message = $"Locator expected to have attribute '{name}'";
        if (expectedValue is Regex)
        {
            message += " matching regex";
        }
        return ExpectImplAsync("to.have.attribute.value", expectedText, expectedValue, message, "Expect \"ToHaveAttributeAsync\"", commonOptions);
    }

    public Task ToHaveClassAsync(string expected, LocatorAssertionsToHaveClassOptions? options = null) =>
        ExpectImplAsync("to.have.class", new ExpectedTextValue() { String = expected }, expected, "Locator expected to have class", "Expect \"ToHaveClassAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveClassAsync(Regex expected, LocatorAssertionsToHaveClassOptions? options = null) =>
        ExpectImplAsync("to.have.class", ExpectedRegex(expected), expected, "Locator expected matching regex", "Expect \"ToHaveClassAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveClassAsync(IEnumerable<string> expected, LocatorAssertionsToHaveClassOptions? options = null) =>
        ExpectImplAsync("to.have.class.array", expected.Select(text => new ExpectedTextValue() { String = text }).ToArray(), expected, "Locator expected to have class", "Expect \"ToHaveClassAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveClassAsync(IEnumerable<Regex> expected, LocatorAssertionsToHaveClassOptions? options = null) =>
        ExpectImplAsync("to.have.class.array", expected.Select(regex => ExpectedRegex(regex)).ToArray(), expected, "Locator expected to have class matching regex", "Expect \"ToHaveClassAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToContainClassAsync(string expected, LocatorAssertionsToContainClassOptions? options = null) =>
        ExpectImplAsync("to.contain.class", new ExpectedTextValue() { String = expected }, expected, "Locator expected to contain class names", "Expect \"ToContainClassAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToContainClassAsync(IEnumerable<string> expected, LocatorAssertionsToContainClassOptions? options = null) =>
        ExpectImplAsync("to.contain.class.array", expected.Select(text => new ExpectedTextValue() { String = text }).ToArray(), expected, "Locator expected to contain class names", "Expect \"ToContainClassAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveCountAsync(int count, LocatorAssertionsToHaveCountOptions? options = null)
    {
        var commonOptions = ConvertToFrameExpectOptions(options);
        commonOptions.ExpectedNumber = count;
        return ExpectImplAsync("to.have.count", null as ExpectedTextValue[], count, "Locator expected to have count", "Expect \"ToHaveCountAsync\"", commonOptions);
    }

    public Task ToHaveCSSAsync(string name, string value, LocatorAssertionsToHaveCSSOptions? options = null) =>
        ToHaveCSSAsync(name, new ExpectedTextValue() { String = value }, value, options);

    public Task ToHaveCSSAsync(string name, Regex value, LocatorAssertionsToHaveCSSOptions? options = null) =>
        ToHaveCSSAsync(name, ExpectedRegex(value), value, options);

    internal Task ToHaveCSSAsync(string name, ExpectedTextValue expectedText, object expectedValue, LocatorAssertionsToHaveCSSOptions? options = null)
    {
        var commonOptions = ConvertToFrameExpectOptions(options);
        commonOptions.ExpressionArg = name;
        var message = $"Locator expected to have CSS property '{name}'";
        if (expectedValue is Regex)
        {
            message += " matching regex";
        }
        return ExpectImplAsync("to.have.css", expectedText, expectedValue, message, "Expect \"ToHaveCSSAsync\"", commonOptions);
    }

    public Task ToHaveIdAsync(string id, LocatorAssertionsToHaveIdOptions? options = null) =>
        ExpectImplAsync("to.have.id", new ExpectedTextValue() { String = id }, id, "Locator expected to have ID", "Expect \"ToHaveIdAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveIdAsync(Regex id, LocatorAssertionsToHaveIdOptions? options = null) =>
        ExpectImplAsync("to.have.id", ExpectedRegex(id), id, "Locator expected to have ID", "Expect \"ToHaveIdAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveJSPropertyAsync(string name, object value, LocatorAssertionsToHaveJSPropertyOptions? options = null)
    {
        var commonOptions = ConvertToFrameExpectOptions(options);
        commonOptions.ExpressionArg = name;
        commonOptions.ExpectedValue = ScriptsHelper.SerializedArgument(value);
        return ExpectImplAsync("to.have.property", null as ExpectedTextValue[], value, $"Locator expected to have JavaScript property '{name}'", "Expect \"ToHaveJSPropertyAsync\"", commonOptions);
    }

    public Task ToHaveTextAsync(string expected, LocatorAssertionsToHaveTextOptions? options = null) =>
        ExpectImplAsync("to.have.text", new ExpectedTextValue() { String = expected, NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false }, expected, "Locator expected to have text", "Expect \"ToHaveTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveTextAsync(Regex expected, LocatorAssertionsToHaveTextOptions? options = null) =>
    ExpectImplAsync("to.have.text", ExpectedRegex(expected, new() { NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false }), expected, "Locator expected to have text matching regex", "Expect \"ToHaveTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveTextAsync(IEnumerable<string> expected, LocatorAssertionsToHaveTextOptions? options = null) =>
        ExpectImplAsync("to.have.text.array", expected.Select(text => new ExpectedTextValue() { String = text, NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false }).ToArray(), expected, "Locator expected to have text", "Expect \"ToHaveTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveTextAsync(IEnumerable<Regex> expected, LocatorAssertionsToHaveTextOptions? options = null) =>
        ExpectImplAsync("to.have.text.array", expected.Select(regex => ExpectedRegex(regex, new() { NormalizeWhiteSpace = true, IgnoreCase = options?.IgnoreCase ?? false })).ToArray(), expected, "Locator expected to have text", "Expect \"ToHaveTextAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveValueAsync(string value, LocatorAssertionsToHaveValueOptions? options = null) =>
        ExpectImplAsync("to.have.value", new ExpectedTextValue() { String = value }, value, "Locator expected to have value", "Expect \"ToHaveValueAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveValueAsync(Regex value, LocatorAssertionsToHaveValueOptions? options = null) =>
        ExpectImplAsync("to.have.value", ExpectedRegex(value), value, "Locator expected to have value matching regex", "Expect \"ToHaveValueAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveValuesAsync(IEnumerable<string> values, LocatorAssertionsToHaveValuesOptions? options = null) =>
        ExpectImplAsync("to.have.values", values.Select(text => new ExpectedTextValue() { String = text }).ToArray(), values, "Locator expected to have values", "Expect \"ToHaveValuesAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveValuesAsync(IEnumerable<Regex> values, LocatorAssertionsToHaveValuesOptions? options = null) =>
        ExpectImplAsync("to.have.values", values.Select(regex => ExpectedRegex(regex)).ToArray(), values, "Locator expected to have matching regex", "Expect \"ToHaveValuesAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveAccessibleDescriptionAsync(string expected, LocatorAssertionsToHaveAccessibleDescriptionOptions? options = null)
        => ExpectImplAsync("to.have.accessible.description", new ExpectedTextValue() { String = expected, IgnoreCase = options?.IgnoreCase, NormalizeWhiteSpace = true }, expected, "Locator expected to have accessible description", "Expect \"ToHaveAccessibleDescriptionAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveAccessibleDescriptionAsync(Regex expected, LocatorAssertionsToHaveAccessibleDescriptionOptions? options = null)
        => ExpectImplAsync("to.have.accessible.description", ExpectedRegex(expected, new() { IgnoreCase = options?.IgnoreCase, NormalizeWhiteSpace = true }), expected, "Locator expected to have accessible description matching regex", "Expect \"ToHaveAccessibleDescriptionAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveAccessibleErrorMessageAsync(string errorMessage, LocatorAssertionsToHaveAccessibleErrorMessageOptions? options = null)
        => ExpectImplAsync("to.have.accessible.error.message", new ExpectedTextValue() { String = errorMessage, IgnoreCase = options?.IgnoreCase }, errorMessage, "Locator expected to have accessible error message", "Expect \"ToHaveAccessibleErrorMessageAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveAccessibleErrorMessageAsync(Regex errorMessage, LocatorAssertionsToHaveAccessibleErrorMessageOptions? options = null)
        => ExpectImplAsync("to.have.accessible.error.message", ExpectedRegex(errorMessage, new() { IgnoreCase = options?.IgnoreCase }), errorMessage, "Locator expected to have accessible error message matching regex", "Expect \"ToHaveAccessibleErrorMessageAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveAccessibleNameAsync(string expected, LocatorAssertionsToHaveAccessibleNameOptions? options = null)
        => ExpectImplAsync("to.have.accessible.name", new ExpectedTextValue() { String = expected, IgnoreCase = options?.IgnoreCase, NormalizeWhiteSpace = true }, expected, "Locator expected to have accessible name", "Expect \"ToHaveAccessibleNameAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveAccessibleNameAsync(Regex expected, LocatorAssertionsToHaveAccessibleNameOptions? options = null)
        => ExpectImplAsync("to.have.accessible.name", ExpectedRegex(expected, new() { IgnoreCase = options?.IgnoreCase, NormalizeWhiteSpace = true }), expected, "Locator expected to have accessible name matching regex", "Expect \"ToHaveAccessibleNameAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToHaveRoleAsync(AriaRole role, LocatorAssertionsToHaveRoleOptions? options = null)
        => ExpectImplAsync("to.have.role", new ExpectedTextValue() { String = role.ToString().ToLowerInvariant() }, role, "Locator expected to have role", "Expect \"ToHaveRoleAsync\"", ConvertToFrameExpectOptions(options));

    public Task ToMatchAriaSnapshotAsync(string expected, LocatorAssertionsToMatchAriaSnapshotOptions? options = null)
    {
        var commonOptions = ConvertToFrameExpectOptions(options);
        commonOptions.ExpectedValue = ScriptsHelper.SerializedArgument(expected);
        return ExpectImplAsync("to.match.aria", null as ExpectedTextValue[], expected, "Locator expected to match Aria snapshot", "Expect \"ToMatchAriaSnapshotAsync\"", commonOptions);
    }
}
