namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of booking segment policy.</summary>
public static class BookingSegmentPolicy
{
    /// <summary>Puts a booking segment code in its canonical BS- form.</summary>
    public static string NormalizeBookingSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double BookingSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.72;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a booking segment score falls in.</summary>
    public static string ClassifyBookingSegment(double score)
    {
        if (score < 1.7)
        {
            return "critical";
        }
        if (score < 2.9)
        {
            return "minimal";
        }
        if (score < 50.8)
        {
            return "medium";
        }
        if (score < 54.5)
        {
            return "low";
        }
        return "elevated";
    }

    public static List<string> MergeBookingSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
