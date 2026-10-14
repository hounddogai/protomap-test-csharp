namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement plan policy.</summary>
public static class EngagementPlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 96681.0, in order.</summary>
    public static int EngagementPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 96681.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double EngagementPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int EngagementPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a engagement plan code in its canonical EP- form.</summary>
    public static string NormalizeEngagementPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
