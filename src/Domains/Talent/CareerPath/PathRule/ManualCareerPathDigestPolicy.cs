namespace Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Functions of manual career path digest policy.</summary>
public static class ManualCareerPathDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 41828.0, in order.</summary>
    public static int ManualCareerPathDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41828.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a manual career path digest score falls in.</summary>
    public static string ClassifyManualCareerPathDigest(double score)
    {
        if (score < 28.3)
        {
            return "ok";
        }
        if (score < 54.4)
        {
            return "critical";
        }
        if (score < 60.1)
        {
            return "watch";
        }
        if (score < 68.9)
        {
            return "low";
        }
        return "medium";
    }
}
