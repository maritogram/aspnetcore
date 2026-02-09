// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers;

public class DisplayNameForTagHelperTest
{
    private const string _displayNameAttributeName = "Display Name";

    private Model _container = new()
    {
        TextWithDisplay = "Container Name",
        Text = "Container No Attribute Name",
    };

    [Fact]
    public void Process_WithDisplayNameAttribute_OutputsAttributeContent()
    {
        const string expectedPreContent = "pre-content";
        const string expectedContent = "content";
        const string expectedTagName = "th";

        var tagHelper = GetTagHelper(model: _container, propertyName: nameof(Model.TextWithDisplay),
            expression: nameof(Model.TextWithDisplay));

        var tagHelperContext = new TagHelperContext(
            tagName: expectedTagName,
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");

        var output = new TagHelperOutput(
            tagName: expectedTagName,
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );

        output.PreContent.SetContent(expectedPreContent);
        output.Content.SetContent(expectedContent);

        // Act
        tagHelper.Process(tagHelperContext, output);

        // Assert
        Assert.Equal(TagMode.StartTagAndEndTag, output.TagMode);
        Assert.Equal(expectedPreContent, output.PreContent.GetContent());
        Assert.Equal(expectedContent, output.Content.GetContent());
        Assert.Equal(_displayNameAttributeName, output.PostContent.GetContent());
        Assert.Equal(expectedTagName, output.TagName);
    }

    [Fact]
    public void Process_WithNoDisplayNameAttribute_OutputsPropertyName()
    {
        var tagHelper = GetTagHelper(model: _container, propertyName: nameof(Model.Text),
            expression: nameof(Model.Text));

        const string expectedPostContent = nameof(Model.Text);

        var tagHelperContext = CreateContext();

        var output = CreateOutput();

        // Act
        tagHelper.Process(tagHelperContext, output);

        // Assert
        Assert.Equal(expectedPostContent, output.PostContent.GetContent());
    }

    [Fact]
    public void Process_WithEmptyDisplayName_OutputsEmptyString()
    {
        // Arrange
        var metadataProvider = new TestModelMetadataProvider();
        metadataProvider
            .ForProperty<Model>(nameof(Model.TextWithDisplay))
            .DisplayDetails(m => m.DisplayName = () => string.Empty);

        var tagHelper = GetTagHelper(_container, nameof(Model.TextWithDisplay), nameof(Model.TextWithDisplay),
            metadataProvider);

        var context = CreateContext();

        var output = CreateOutput();

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal(string.Empty, output.PostContent.GetContent());
    }

    [Fact]
    public void Process_WithNullDisplayNameAttribute_OutputsPropertyName()
    {
        // Arrange
        var metadataProvider = new TestModelMetadataProvider();
        metadataProvider
            .ForProperty<Model>(nameof(Model.TextWithDisplay))
            .DisplayDetails(m => m.DisplayName = () => null);

        var tagHelper = GetTagHelper(_container, nameof(Model.TextWithDisplay), nameof(Model.TextWithDisplay),
            metadataProvider);

        var output = CreateOutput();

        var context = CreateContext();

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal(nameof(Model.TextWithDisplay), output.PostContent.GetContent());
    }

    [Theory]
    [InlineData("A.B", "B")]
    [InlineData("A.B.C", "C")]
    [InlineData("A.B.C.D.E", "E")]
    public void Process_WithNullResolvedDisplayNameAndDottedExpression_UsesLastSegment(
        string expression, string expectedPostContent)
    {
        // Arrange
        var metadataProvider = new TestModelMetadataProvider();

        var typeExplorer =
            metadataProvider.GetModelExplorerForType(typeof(string), "test-value");

        var modelExpression =
            new ModelExpression(expression, typeExplorer);

        var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
        var tagHelper = new DisplayNameForTagHelper(htmlGenerator) { For = modelExpression };

        var output = CreateOutput();

        var context = CreateContext();

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal(expectedPostContent, output.PostContent.GetContent());
    }

    [Fact]
    public void Process_WithNullResolvedDisplayName_UsesExpression()
    {
        // Arrange
        var metadataProvider = new TestModelMetadataProvider();

        var typeExplorer =
            metadataProvider.GetModelExplorerForType(typeof(string), "test-value");

        var modelExpression =
            new ModelExpression($"{nameof(Model.Text)}", typeExplorer);

        var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
        var tagHelper = new DisplayNameForTagHelper(htmlGenerator) { For = modelExpression };

        var output = CreateOutput();

        var context = CreateContext();

        // Act
        tagHelper.Process(context, output);

        // Assert
        Assert.Equal(nameof(Model.Text), output.PostContent.GetContent());
    }

    private static DisplayNameForTagHelper GetTagHelper(
        Model model,
        string propertyName,
        string expression,
        IModelMetadataProvider metadataProvider = null
    )
    {
        metadataProvider ??= new TestModelMetadataProvider();

        var containerExplorer = metadataProvider.GetModelExplorerForType(typeof(Model), model);

        var propertyMetadata =
            metadataProvider.GetMetadataForProperty(typeof(Model), propertyName);
        var propertyExplorer = containerExplorer.GetExplorerForExpression(propertyMetadata, model);

        var modelExpression = new ModelExpression(expression, propertyExplorer);
        var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
        var tagHelper =
            new DisplayNameForTagHelper(htmlGenerator) { For = modelExpression };

        return tagHelper;
    }

    private static TagHelperContext CreateContext()
    {
        return new TagHelperContext(
            tagName: "th",
            allAttributes: new TagHelperAttributeList(),
            items: new Dictionary<object, object>(),
            uniqueId: "test");
    }

    private static TagHelperOutput CreateOutput()
    {
        return new TagHelperOutput(
            tagName: "th",
            attributes: new TagHelperAttributeList(),
            getChildContentAsync: (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
    }

    private class Model
    {
        [Display(Name = _displayNameAttributeName)]
        public string TextWithDisplay { get; set; }

        public string Text { get; set; }
    }
}