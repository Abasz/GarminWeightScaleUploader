using GarminConnectClient.Lib.Dto;
using GarminWeightScaleUploader.Lib.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GarminWeightScaleUploader.Lib;

public class GarminWeightScaleUploader
{
    private static readonly string _tmpDir = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp")).FullName;

    private static GarminConnectClient.Lib.IConfiguration SetupConfiguration(string userName, string password)
    {
        var builder = new ConfigurationBuilder()
        .AddInMemoryCollection(
        new Dictionary<string, string>
        {
            ["AppConfig:Username"] = userName,
            ["AppConfig:Password"] = password,
            ["AppConfig:BackupDir"] = _tmpDir,
        });
        IConfigurationRoot configuration = builder.Build();
        return new Configuration(configuration);
    }

    private static string CreateWeightScaleFitFile(GarminWeightScaleData garminWeightScaleData, UserProfileSettings userProfileSettings)
    {
        var timeCreated = new Dynastream.Fit.DateTime(System.DateTime.UtcNow);
        var outputFilePath = Path.Combine(_tmpDir, $"{timeCreated.GetTimeStamp()}_WEIGHT_SCALE.fit");

        var stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        var encoder = new Encode(ProtocolVersion.V20);

        encoder.Open(stream);

        var fileIdMesg = new FileIdMesg();
        fileIdMesg.SetType(Dynastream.Fit.File.Weight);
        fileIdMesg.SetManufacturer(Manufacturer.Garmin);
        fileIdMesg.SetGarminProduct(2429);
        fileIdMesg.SetSerialNumber(1234);
        fileIdMesg.SetTimeCreated(timeCreated);
        encoder.Write(fileIdMesg);

        var myUserProfile = new UserProfileMesg();
        myUserProfile.SetGender(userProfileSettings.Gender);
        myUserProfile.SetAge(Convert.ToByte(userProfileSettings.Age));
        myUserProfile.SetWeight(garminWeightScaleData.Weight);
        myUserProfile.SetHeight(userProfileSettings.Height / 100);
        myUserProfile.SetActivityClass(userProfileSettings.ActivityClass);
        myUserProfile.SetMessageIndex(0);
        myUserProfile.SetLocalId(0);

        encoder.Write(myUserProfile);

        var weightMesg = new WeightScaleMesg();

        weightMesg.SetTimestamp(new Dynastream.Fit.DateTime(garminWeightScaleData.TimeStamp.ToUniversalTime()));
        weightMesg.SetUserProfileIndex(0);
        weightMesg.SetWeight(garminWeightScaleData.Weight);

        if (garminWeightScaleData.PercentFat is not null)
            weightMesg.SetPercentFat(garminWeightScaleData.PercentFat);
        if (garminWeightScaleData.PercentHydration is not null)
            weightMesg.SetPercentHydration(garminWeightScaleData.PercentHydration);
        if (garminWeightScaleData.MuscleMass is not null)
            weightMesg.SetMuscleMass(garminWeightScaleData.MuscleMass);
        if (garminWeightScaleData.BoneMass is not null)
            weightMesg.SetBoneMass(garminWeightScaleData.BoneMass);
        if (garminWeightScaleData.MetabolicAge is not null)
            weightMesg.SetMetabolicAge(garminWeightScaleData.MetabolicAge);
        if (garminWeightScaleData.PhysiqueRating is not null)
            weightMesg.SetPhysiqueRating(garminWeightScaleData.PhysiqueRating);
        if (garminWeightScaleData.VisceralFatMass is not null)
            weightMesg.SetVisceralFatMass(garminWeightScaleData.VisceralFatMass);
        if (garminWeightScaleData.VisceralFatRating is not null)
            weightMesg.SetVisceralFatRating(garminWeightScaleData.VisceralFatRating);


        if (garminWeightScaleData.Weight > 0 && userProfileSettings.Height > 0 || garminWeightScaleData.BodyMassIndex is not null)
            weightMesg.SetBmi((float)Math.Round(garminWeightScaleData.BodyMassIndex ?? (garminWeightScaleData.Weight / Math.Pow((float)userProfileSettings.Height! / 100, 2)), 1));

        encoder.Write(weightMesg);

        var deviceInfoMesg = new DeviceInfoMesg();
        deviceInfoMesg.SetTimestamp(timeCreated);
        deviceInfoMesg.SetBatteryVoltage(384);
        deviceInfoMesg.SetCumOperatingTime(45126);

        encoder.Close();
        stream.Close();

        return outputFilePath;
    }

    async public static Task<bool> UploadAsync(GarminWeightScaleDTO weightScaleDTO, UserProfileSettings userProfileSettings)
    {
        if (string.IsNullOrWhiteSpace(weightScaleDTO.Email))
            throw new InvalidOperationException("Garmin login e-mail address is required");
        if (string.IsNullOrWhiteSpace(weightScaleDTO.Password))
            throw new InvalidOperationException("Garmin login password is required");

        var fitFilePath = CreateWeightScaleFitFile(weightScaleDTO, userProfileSettings);

        var configuration = SetupConfiguration(weightScaleDTO.Email, weightScaleDTO.Password);
        var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<Client>();
        var client = new Client(configuration, logger);
        await client.Authenticate();
        if (!(await client.UploadActivity(fitFilePath, new FileFormat { FormatKey = "fit" })).Success)
        {
            throw new Exception("Error while uploading uploading Garmin Connect");
        }
        return true;
    }
}
