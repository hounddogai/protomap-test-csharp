namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan slot policy.</summary>
public static class ReturnPlanSlotPolicy
{
    /// <summary>Names the band a return plan slot score falls in.</summary>
    public static string ClassifyReturnPlanSlot(double score)
    {
        if (score < 26.5)
        {
            return "low";
        }
        if (score < 54.2)
        {
            return "watch";
        }
        if (score < 97.3)
        {
            return "minimal";
        }
        return "elevated";
    }

    public static string ReturnPlanSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 79301.0, in order.</summary>
    public static int ReturnPlanSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79301.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a return plan slot code in its canonical RPS- form.</summary>
    public static string NormalizeReturnPlanSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
