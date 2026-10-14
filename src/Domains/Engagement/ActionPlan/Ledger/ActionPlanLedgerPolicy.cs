namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan ledger policy.</summary>
public static class ActionPlanLedgerPolicy
{
    /// <summary>Puts a action plan ledger code in its canonical APL- form.</summary>
    public static string NormalizeActionPlanLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ActionPlanLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 12546.0, in order.</summary>
    public static int ActionPlanLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12546.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
