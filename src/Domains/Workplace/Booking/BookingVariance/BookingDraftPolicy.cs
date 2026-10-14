namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of booking draft policy.</summary>
public static class BookingDraftPolicy
{
    public static List<string> MergeBookingDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a booking draft score falls in.</summary>
    public static string ClassifyBookingDraft(double score)
    {
        if (score < 47.8)
        {
            return "ok";
        }
        if (score < 87.1)
        {
            return "critical";
        }
        return "high";
    }

    public static double BookingDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
