namespace GarminWeightScaleUploader.Lib.Models;

public record UserProfileSettings
{
    /// <summary>
    /// Gender enum as per the FIT SDK
    /// </summary>
    /// <value>Male</value>
    /// <summary>
    /// Height in centimeter
    /// </summary>
    /// <value></value>
    /// <summary>
    /// Activity class enum that is in reality a byte type with possible values between either 1 and 100 or equal to 128 for professional athletes
    /// </summary>
    /// <returns></returns>
}

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
    public byte? PhysiqueRating { init; get; }
    public byte? MetabolicAge { init; get; }
    public float? BodyMassIndex { init; get; }
}

public record GarminWeightScaleDTO : GarminWeightScaleData
{
    public string Email { init; get; } = default!;
    public string Password { init; get; } = default!;
}