namespace Acme.Workforce.Talent.Mobility.Review;

/// <summary>Functions of scheduled mobility review policy.</summary>
public static class ScheduledMobilityReviewPolicy
{
    /// <summary>Puts a scheduled mobility review code in its canonical SMR- form.</summary>
    public static string NormalizeScheduledMobilityReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scheduled mobility review score falls in.</summary>
    public static string ClassifyScheduledMobilityReview(double score)
    {
        if (score < 17.7)
        {
            return "medium";
        }
        if (score < 36.9)
        {
            return "low";
        }
        return "watch";
    }

    public static int ScheduledMobilityReviewSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 61973.0, in order.</summary>
    public static int ScheduledMobilityReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61973.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
