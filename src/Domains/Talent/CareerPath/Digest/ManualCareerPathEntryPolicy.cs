namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual career path entry policy.</summary>
public static class ManualCareerPathEntryPolicy
{
    /// <summary>Names the band a manual career path entry score falls in.</summary>
    public static string ClassifyManualCareerPathEntry(double score)
    {
        if (score < 17.3)
        {
            return "minimal";
        }
        if (score < 42.0)
        {
            return "high";
        }
        if (score < 44.1)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 48581.0, in order.</summary>
    public static int ManualCareerPathEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48581.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int ManualCareerPathEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string ManualCareerPathEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
