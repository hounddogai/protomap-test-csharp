namespace Acme.Workforce.Workplace.Booking.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered booking assignment policy.</summary>
public static class TieredBookingAssignmentPolicy
{
    public static List<string> MergeTieredBookingAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered booking assignment score falls in.</summary>
    public static string ClassifyTieredBookingAssignment(double score)
    {
        if (score < 6.8)
        {
            return "critical";
        }
        if (score < 22.9)
        {
            return "watch";
        }
        if (score < 27.5)
        {
            return "elevated";
        }
        return "high";
    }

    public static double TieredBookingAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
