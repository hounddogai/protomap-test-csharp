namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict desk budget policy.</summary>
public static class StrictDeskBudgetPolicy
{
    /// <summary>Puts a strict desk budget code in its canonical SDB- form.</summary>
    public static string NormalizeStrictDeskBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SDB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SDB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string StrictDeskBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 13821.0, in order.</summary>
    public static int StrictDeskBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13821.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
