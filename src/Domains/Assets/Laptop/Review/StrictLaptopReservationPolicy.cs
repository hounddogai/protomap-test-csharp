namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict laptop reservation policy.</summary>
public static class StrictLaptopReservationPolicy
{
    /// <summary>Names the band a strict laptop reservation score falls in.</summary>
    public static string ClassifyStrictLaptopReservation(double score)
    {
        if (score < 36.4)
        {
            return "high";
        }
        if (score < 86.1)
        {
            return "medium";
        }
        if (score < 95.5)
        {
            return "minimal";
        }
        return "elevated";
    }

    public static double StrictLaptopReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
