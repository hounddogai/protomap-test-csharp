namespace Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Functions of offboarding manifest policy.</summary>
public static class OffboardingManifestPolicy
{
    public static int OffboardingManifestSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 8852.0, in order.</summary>
    public static int OffboardingManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8852.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a offboarding manifest score falls in.</summary>
    public static string ClassifyOffboardingManifest(double score)
    {
        if (score < 14.5)
        {
            return "low";
        }
        if (score < 24.8)
        {
            return "minimal";
        }
        if (score < 33.0)
        {
            return "watch";
        }
        return "critical";
    }
}
