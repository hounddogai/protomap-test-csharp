namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan preference policy.</summary>
public static class ActionPlanPreferencePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10712.0, in order.</summary>
    public static int ActionPlanPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10712.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a action plan preference code in its canonical APP- form.</summary>
    public static string NormalizeActionPlanPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ActionPlanPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double ActionPlanPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
