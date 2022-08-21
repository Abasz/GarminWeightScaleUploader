# Garmin Weight Scale Measurement Uploader

The purpose of this class library to provide a simple API for a [MiScale Exporter](https://github.com/lswiderski/mi-scale-exporter) for interfacing garmin connect to upload weight data.

## Usage

The library has only one static method that needs the DTO and the userProfileSetting. It returns true if upload is successful otherwise throws the appropriate error.
```cs
GarminWeightScaleUploader.UploadAsync(weightScaleDTO, userProfileSettings);
```

## Models

There are three model classes to to hold the necessary data:

### UserProfileSettings
 ```cs
public record UserProfileSettings
{
    public Gender Gender { init; get; } = Gender.Male;
    public int? Age { init; get; }
    public int Height { init; get; }
    public ActivityClass ActivityClass { init; get; } = (ActivityClass)90;
}
```
Where the Gender and ActivityClass are per the C# implementation of the FIT SDK and Height should be in centimeters.

### GarminWeightScaleData
```cs
public record GarminWeightScaleData
{
    public System.DateTime TimeStamp { init; get; }
    public float Weight { init; get; }
    public float? PercentFat { init; get; }
    public float? PercentHydration { init; get; }
    public float? BoneMass { init; get; }
    public float? MuscleMass { init; get; }
    public byte? VisceralFatRating { init; get; }
    public float? VisceralFatMass { init; get; }
    public int? PhysiqueRating { init; get; }
    public float? MetabolicAge { init; get; }
    public float? BodyMassIndex { init; get; }
}
```
Where the type of each element corresponds to the respective type as per the FIT SDK

### GarminWeightScaleDTO
```cs
public record GarminWeightScaleDTO : GarminWeightScaleData
{
    public string Email { init; get; } = default!;
    public string Password { init; get; } = default!;
}
```
A class that includes the credentials necessary for logging into Garmin Connect along with the Weight Scale data.