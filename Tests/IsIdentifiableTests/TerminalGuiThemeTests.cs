using ii;
using NUnit.Framework;
using Terminal.Gui;
using YamlDotNet.Serialization;

namespace IsIdentifiable.Tests;

class TerminalGuiThemeTests
{
    [Test]
    public void TestDeserialization()
    {
        var themeFile = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "theme.yaml");
        var des = new Deserializer();

        var theme = des.Deserialize<TerminalGuiTheme>(System.IO.File.ReadAllText(themeFile));

        Assert.Multiple(() =>
        {
            Assert.That(theme.Base.HotFocusBackground, Is.Not.EqualTo(default(Color)));
            Assert.That(theme.Base.HotFocusForeground, Is.Not.EqualTo(default(Color)));
            Assert.That(theme.Base.FocusForeground, Is.EqualTo(Color.Black));
            Assert.That(theme.Base.FocusBackground, Is.Not.EqualTo(default(Color)));
            Assert.That(theme.Base.HotNormalBackground, Is.Not.EqualTo(default(Color)));
            Assert.That(theme.Base.HotNormalForeground, Is.Not.EqualTo(default(Color)));
        });

        theme = new TerminalGuiTheme();

        Assert.Multiple(() =>
        {
            Assert.That(theme.Base.HotFocusBackground, Is.EqualTo(default(Color)));
            Assert.That(theme.Base.HotFocusForeground, Is.EqualTo(default(Color)));
            Assert.That(theme.Base.FocusForeground, Is.EqualTo(default(Color)));
            Assert.That(theme.Base.FocusBackground, Is.EqualTo(default(Color)));
            Assert.That(theme.Base.HotNormalBackground, Is.EqualTo(default(Color)));
            Assert.That(theme.Base.HotNormalForeground, Is.EqualTo(default(Color)));
        });

    }
}
