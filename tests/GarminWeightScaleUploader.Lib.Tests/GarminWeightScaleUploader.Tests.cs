using FitFileConverter.ClassLibrary;
using GarminWeightScaleUploader.Lib.Models;

namespace GarminWeightScaleUploader.Lib.Tests;

public class GarminWeightScaleUploaderTests : IDisposable
{
    private readonly UserProfileSettings _userProfileSettings;
    private readonly GarminWeightScaleDTO _garminWeightScaleDTO;
    private readonly string _tmpDir;

    public GarminWeightScaleUploaderTests()
    {
        _tmpDir = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp")).FullName;
        _userProfileSettings = new UserProfileSettings
        {
            Age = 40,
            Height = 180,
        };
        _garminWeightScaleDTO = new GarminWeightScaleDTO
        {
            TimeStamp = DateTime.UtcNow,
            Weight = 82.1f,
            PercentFat = 10.1f,
            PercentHydration = 53.3f,
            BoneMass = 5.8f,
            MuscleMass = 32f,
            VisceralFatRating = 9,
            VisceralFatMass = 10f,
            PhysiqueRating = 9,
            MetabolicAge = 28,
            Email = Environment.GetEnvironmentVariable("EMAIL")!,
            Password = Environment.GetEnvironmentVariable("PASSWORD")!
        };
    }

    public void Dispose()
    {
        Directory.Delete(_tmpDir, true);
    }

    [Fact(DisplayName = "Email should be included in DTO")]
    public async Task EmailShouldNotBeNull()
    {
        var data = _garminWeightScaleDTO with { Email = null! };
        await Assert.ThrowsAsync<InvalidOperationException>(() => GarminWeightScaleUploader.UploadAsync(data, _userProfileSettings));
    }

    [Fact(DisplayName = "Password should be included in DTO")]
    public async Task PasswordShouldNotBeNull()
    {
        var data = _garminWeightScaleDTO with { Password = null! };
        await Assert.ThrowsAsync<InvalidOperationException>(() => GarminWeightScaleUploader.UploadAsync(data, _userProfileSettings));
    }

    [Fact(DisplayName = "Weight scale files should be created")]
    public async Task FitFileShouldBeCreated()
    {
        var data = _garminWeightScaleDTO with { Email = "test", Password = "test" };

        try
        {
            await GarminWeightScaleUploader.UploadAsync(data, _userProfileSettings);
        }
        catch
        {
            var files = Directory.GetFiles(_tmpDir);
            Assert.Collection(files, item => Assert.Contains(".fit", item));
        }
    }

    [Fact(DisplayName = "Weight scale files should include appropriate data")]
    public async Task FitFileDataShouldBeValid()
    {
        var data = _garminWeightScaleDTO with { Email = "test", Password = "test" };

        try
        {
            await GarminWeightScaleUploader.UploadAsync(data, _userProfileSettings);
        }
        catch
        {
            var fitFile = Directory.GetFiles(_tmpDir).First(item => item.Contains(".fit"));
            var fitData = FitFileParser.FromFit(fitFile);

            Assert.Single(fitData.WeightScaleMesgs);
            Assert.Collection(fitData.WeightScaleMesgs, mesg =>
            {
                Assert.Equal(_garminWeightScaleDTO.Weight, mesg.GetWeight());
                Assert.Equal(MathF.Round(_garminWeightScaleDTO.Weight / MathF.Pow((float)_userProfileSettings.Height! / 100, 2), 1), mesg.GetBmi());
            });

        }
    }

    [Fact(DisplayName = "Weight scale files should include appropriate BMI if provided")]
    public async Task BMIShouldBeAdded()
    {
        var data = _garminWeightScaleDTO with { BodyMassIndex = 10.1212f, Email = "test", Password = "test" };
        var userData = _userProfileSettings with { Height = null! };

        try
        {
            await GarminWeightScaleUploader.UploadAsync(data, userData);
        }
        catch
        {
            var fitFile = Directory.GetFiles(_tmpDir).First(item => item.Contains(".fit"));
            var fitData = FitFileParser.FromFit(fitFile);

            Assert.Single(fitData.WeightScaleMesgs);
            Assert.Collection(fitData.WeightScaleMesgs, mesg =>
            {
                Assert.Equal(MathF.Round((float)data.BodyMassIndex!, 1), mesg.GetBmi());
            });

        }
    }

    [Fact(DisplayName = "Create file without BMI")]
    public async Task BMINullShouldNotBeAdded()
    {
        var data = _garminWeightScaleDTO with { Email = "test", Password = "test" };
        var userData = _userProfileSettings with { Height = null! };

        try
        {
            await GarminWeightScaleUploader.UploadAsync(data, userData);
        }
        catch
        {
            var fitFile = Directory.GetFiles(_tmpDir).First(item => item.Contains(".fit"));
            var fitData = FitFileParser.FromFit(fitFile);

            Assert.Single(fitData.WeightScaleMesgs);
            Assert.Collection(fitData.WeightScaleMesgs, mesg =>
            {
                Assert.Null(mesg.GetBmi());
            });

        }
    }

    [Fact(DisplayName = "Throw error when credentials are invalid")]
    public async Task InvalidCredentials()
    {
        var data = _garminWeightScaleDTO with { Email = "test", Password = "test" };

        var exception = await Record.ExceptionAsync(() => GarminWeightScaleUploader.UploadAsync(data, _userProfileSettings));

        Assert.Contains("Unauthorized", exception.Message);
    }

    [Fact(DisplayName = "Return success status when upload is completed")]
    public async Task SuccessfullUpload()
    {
        var response = await GarminWeightScaleUploader.UploadAsync(_garminWeightScaleDTO, _userProfileSettings);

        Assert.True(response);
    }
}