namespace Acme.Workforce.Workplace.Booking.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive booking exception policy.</summary>
public static class AdaptiveBookingExceptionPolicy
{
    public static List<string> MergeAdaptiveBookingExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double AdaptiveBookingExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AdaptiveBookingExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a adaptive booking exception score falls in.</summary>
    public static string ClassifyAdaptiveBookingException(double score)
    {
        if (score < 17.7)
        {
            return "medium";
        }
        if (score < 71.2)
        {
            return "watch";
        }
        if (score < 91.7)
        {
            return "elevated";
        }
        return "low";
    }
}
