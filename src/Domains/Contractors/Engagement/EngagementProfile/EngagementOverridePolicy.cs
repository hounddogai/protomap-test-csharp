namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement override policy.</summary>
public static class EngagementOverridePolicy
{
    /// <summary>Names the band a engagement override score falls in.</summary>
    public static string ClassifyEngagementOverride(double score)
    {
        if (score < 13.6)
        {
            return "elevated";
        }
        if (score < 33.0)
        {
            return "low";
        }
        if (score < 63.7)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Puts a engagement override code in its canonical EO- form.</summary>
    public static string NormalizeEngagementOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double EngagementOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.65;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 84992.0, in order.</summary>
    public static int EngagementOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 84992.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
