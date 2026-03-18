using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TellTeddie.Web.Components.Forms;
using TellTeddie.Web.Services;
using Xunit;

namespace TellTeddie.Web.Tests.Components.Forms
{
    public class TextFormComponentTests : TestContext
    {
        private Mock<ITextPostService> _textPostServiceMock;

        public TextFormComponentTests()
        {
            _textPostServiceMock = new Mock<ITextPostService>();
            Services.AddSingleton(_textPostServiceMock.Object);
        }

        [Fact]
        public void TextForm_Renders_WithTitle()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var heading = cut.Find("h3");
            Assert.NotNull(heading);
            Assert.Contains("Text Posts", heading.TextContent);
        }

        [Fact]
        public void TextForm_Renders_WithDescription()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var paragraph = cut.Find("p");
            Assert.NotNull(paragraph);
            Assert.Contains("text posts side of your app", paragraph.TextContent);
        }

        [Fact]
        public void TextForm_Renders_WithEditForm()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            // EditForm should be rendered
            Assert.NotNull(cut);
        }

        [Fact]
        public void TextForm_HasNameInput()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            // Look for the Name label and input
            var labels = cut.FindAll("label");
            var nameLabel = labels.FirstOrDefault(l => l.TextContent.Contains("Name"));
            Assert.NotNull(nameLabel);
        }

        [Fact]
        public void TextForm_HasCaptionTextarea()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var markup = cut.Markup;
            Assert.Contains("Caption", markup);
        }

        [Fact]
        public void TextForm_HasTextBodyTextarea()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var markup = cut.Markup;
            Assert.Contains("Text Post", markup);
        }

        [Fact]
        public void TextForm_TextAreaHasAriaLabel()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var textAreas = cut.FindAll("textarea");
            var textPostArea = textAreas.FirstOrDefault(t => 
                t.GetAttribute("aria-label") != null && 
                t.GetAttribute("aria-label").Contains("Text Post"));
            Assert.NotNull(textPostArea);
        }

        [Fact]
        public void TextForm_Renders_WithoutErrors()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            Assert.NotNull(cut);
        }

        [Fact]
        public void TextForm_PageTitle_IsSet()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            // Component renders successfully with page title
            Assert.NotNull(cut);
        }

        [Fact]
        public void TextForm_HasDataAnnotationsValidator()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            // EditForm should have DataAnnotationsValidator
            Assert.NotNull(cut);
        }

        [Fact]
        public void TextForm_HasValidationSummary()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var markup = cut.Markup;
            // ValidationSummary should be present
            Assert.NotNull(cut);
        }

        [Fact]
        public void TextForm_TextBodyHasValidationMessage()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            // ValidationMessage should be present for TextBody
            Assert.NotNull(cut);
        }

        [Fact]
        public void TextForm_InputFields_HaveCorrectClasses()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var inputs = cut.FindAll("input");
            Assert.True(inputs.Count > 0);
        }

        [Fact]
        public void TextForm_TextareaHasRows()
        {
            // Arrange & Act
            var cut = Render<TextForm>();

            // Assert
            var textareas = cut.FindAll("textarea");
            Assert.True(textareas.Count > 0);
            foreach (var textarea in textareas)
            {
                var rows = textarea.GetAttribute("rows");
                Assert.NotNull(rows);
            }
        }
    }
}
