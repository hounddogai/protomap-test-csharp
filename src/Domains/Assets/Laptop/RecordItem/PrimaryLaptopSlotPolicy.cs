namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary laptop slot policy.</summary>
public static class PrimaryLaptopSlotPolicy
{
    /// <summary>Names the band a primary laptop slot score falls in.</summary>
    public static string ClassifyPrimaryLaptopSlot(double score)
    {
        if (score < 16.0)
        {
            return "critical";
        }
        if (score < 73.9)
        {
            return "watch";
        }
        if (score < 91.5)
        {
            return "minimal";
        }
        return "elevated";
    }

    public static double PrimaryLaptopSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
