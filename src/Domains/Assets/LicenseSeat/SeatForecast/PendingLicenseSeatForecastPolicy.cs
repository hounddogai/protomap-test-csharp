namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending license seat forecast policy.</summary>
public static class PendingLicenseSeatForecastPolicy
{
    /// <summary>Names the band a pending license seat forecast score falls in.</summary>
    public static string ClassifyPendingLicenseSeatForecast(double score)
    {
        if (score < 49.5)
        {
            return "critical";
        }
        if (score < 83.6)
        {
            return "watch";
        }
        if (score < 86.6)
        {
            return "high";
        }
        return "elevated";
    }

    public static double PendingLicenseSeatForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePendingLicenseSeatForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
