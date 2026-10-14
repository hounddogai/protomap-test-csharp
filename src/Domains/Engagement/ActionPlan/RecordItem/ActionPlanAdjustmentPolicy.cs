namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Functions of action plan adjustment policy.</summary>
public static class ActionPlanAdjustmentPolicy
{
    /// <summary>Names the band a action plan adjustment score falls in.</summary>
    public static string ClassifyActionPlanAdjustment(double score)
    {
        if (score < 15.4)
        {
            return "low";
        }
        if (score < 57.5)
        {
            return "watch";
        }
        return "elevated";
    }

    public static int ActionPlanAdjustmentSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a action plan adjustment code in its canonical APA- form.</summary>
    public static string NormalizeActionPlanAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 67627.0, in order.</summary>
    public static int ActionPlanAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67627.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
