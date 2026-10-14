namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling laptop claim policy.</summary>
public static class RollingLaptopClaimPolicy
{
    public static string RollingLaptopClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a rolling laptop claim score falls in.</summary>
    public static string ClassifyRollingLaptopClaim(double score)
    {
        if (score < 35.2)
        {
            return "minimal";
        }
        if (score < 43.0)
        {
            return "low";
        }
        if (score < 50.2)
        {
            return "watch";
        }
        return "elevated";
    }

    public static int RollingLaptopClaimSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    public static double RollingLaptopClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
