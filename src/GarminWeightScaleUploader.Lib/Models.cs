
namespace GarminWeightScaleUploader.Lib.Models;

public record UserProfileSettings
{
    public Gender Gender { init; get; } = Gender.Male;
    public int? Age { init; get; }
    /// <summary>
    /// Height in centimeter
    /// </summary>
    /// <value></value>
    public int? Height { init; get; }
    public ActivityClass ActivityClass { init; get; } = (ActivityClass)90;
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