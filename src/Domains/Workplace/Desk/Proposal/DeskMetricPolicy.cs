namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk metric policy.</summary>
public static class DeskMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 49967.0, in order.</summary>
    public static int DeskMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49967.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a desk metric code in its canonical DM- form.</summary>
    public static string NormalizeDeskMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string DeskMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
