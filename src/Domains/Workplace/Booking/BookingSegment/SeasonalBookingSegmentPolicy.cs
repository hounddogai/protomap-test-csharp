namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal booking segment policy.</summary>
public static class SeasonalBookingSegmentPolicy
{
    /// <summary>Names the band a seasonal booking segment score falls in.</summary>
    public static string ClassifySeasonalBookingSegment(double score)
    {
        if (score < 37.4)
        {
            return "elevated";
        }
        if (score < 43.4)
        {
            return "watch";
        }
        if (score < 81.8)
        {
            return "medium";
        }
        return "ok";
    }

    public static double SeasonalBookingSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 5485.0, in order.</summary>
    public static int SeasonalBookingSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5485.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeSeasonalBookingSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
