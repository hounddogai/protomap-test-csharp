namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan digest policy.</summary>
public static class ActionPlanDigestPolicy
{
    /// <summary>Puts a action plan digest code in its canonical APD- form.</summary>
    public static string NormalizeActionPlanDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ActionPlanDigestSpan(int start, int end)
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
        return span / 86400;
    }

    public static double ActionPlanDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 66815.0, in order.</summary>
    public static int ActionPlanDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66815.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
