namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default action plan preference policy.</summary>
public static class DefaultActionPlanPreferencePolicy
{
    public static string DefaultActionPlanPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a default action plan preference code in its canonical DAPP- form.</summary>
    public static string NormalizeDefaultActionPlanPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DAPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DAPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default action plan preference score falls in.</summary>
    public static string ClassifyDefaultActionPlanPreference(double score)
    {
        if (score < 48.4)
        {
            return "elevated";
        }
        if (score < 58.7)
        {
            return "ok";
        }
        return "high";
    }

    public static int DefaultActionPlanPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
