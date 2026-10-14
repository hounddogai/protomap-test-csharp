namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual booking preference policy.</summary>
public static class ManualBookingPreferencePolicy
{
    /// <summary>Names the band a manual booking preference score falls in.</summary>
    public static string ClassifyManualBookingPreference(double score)
    {
        if (score < 38.1)
        {
            return "low";
        }
        if (score < 52.9)
        {
            return "high";
        }
        if (score < 67.1)
        {
            return "elevated";
        }
        if (score < 74.8)
        {
            return "medium";
        }
        return "critical";
    }

    public static List<string> MergeManualBookingPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int ManualBookingPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a manual booking preference code in its canonical MBP- form.</summary>
    public static string NormalizeManualBookingPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
