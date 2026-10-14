namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual policy preference policy.</summary>
public static class ManualPolicyPreferencePolicy
{
    public static double ManualPolicyPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a manual policy preference code in its canonical MPP- form.</summary>
    public static string NormalizeManualPolicyPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a manual policy preference score falls in.</summary>
    public static string ClassifyManualPolicyPreference(double score)
    {
        if (score < 36.2)
        {
            return "low";
        }
        if (score < 46.1)
        {
            return "high";
        }
        if (score < 67.2)
        {
            return "minimal";
        }
        if (score < 87.2)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 26671.0, in order.</summary>
    public static int ManualPolicyPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26671.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
