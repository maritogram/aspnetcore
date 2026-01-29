// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers;

/// <summary>
/// <see cref="ITagHelper"/> implementation targeting &lt;display-name-for&gt; elements that renders
/// the display name for a specified model expression.
/// </summary>
[HtmlTargetElement("*", Attributes = ForAttributeName)]
public class DisplayNameForTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-display-name-for";

    /// <summary>
    /// Gets the <see cref="IHtmlGenerator"/> used to generate the <see cref="DisplayNameForTagHelper"/>'s output.
    /// </summary>
    protected IHtmlGenerator HtmlGenerator { get; }

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set;} 

    /// <summary>
    /// Creates a new <see cref="DisplayNameForTagHelper"/>.  
    /// </summary>
    public DisplayNameForTagHelper(IHtmlGenerator htmlGenerator)
    {
        HtmlGenerator = htmlGenerator;
    }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        output.PostContent.AppendHtml(
            HtmlGenerator.GenerateDisplayName(For.ModelExplorer, For.Name));
    }

}