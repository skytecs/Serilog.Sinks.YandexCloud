using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Sinks.YandexCloud.Tests.Unit;
public class YandexCloudSinkSettingsTests
{
    [Test]
    public void ShouldThrowIfLogGroupIdOrFolderIdIsNotSpecified()
    {
        var settings = new YandexCloudSinkSettings();
        Assert.That(() => settings.Validate(), Throws.ArgumentException);
    }

    [Test]
    public void ShouldThrowIfBothLogGroupIdAndFolderIdAreSpecified()
    {
        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
            FolderId = Guid.NewGuid().ToString(),
        };
        Assert.That(() => settings.Validate(), Throws.ArgumentException);
    }

    [Test]
    public void ShouldThrowIfResourceTypeIsInIncorrectFormat()
    {
        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
            ResourceType = "Resource Type",
        };
        Assert.That(() => settings.Validate(), Throws.ArgumentException);
    }

    [Test]
    public void ShouldThrowIfResourceIdIsInIncorrectFormat()
    {
        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = Guid.NewGuid().ToString(),
            ResourceId = "Resource Id",
        };
        Assert.That(() => settings.Validate(), Throws.ArgumentException);
    }

    [Test]
    public void ShouldThrowIfLogGroupIdIsInIncorrectFormat()
    {
        var settings = new YandexCloudSinkSettings
        {
            LogGroupId = "Log Group Id",
        };
        Assert.That(() => settings.Validate(), Throws.ArgumentException);
    }

    [Test]
    public void ShouldThrowIfFolderIdIsInIncorrectFormat()
    {
        var settings = new YandexCloudSinkSettings
        {
            FolderId = "Folder Id",
        };
        Assert.That(() => settings.Validate(), Throws.ArgumentException);
    }
}
