namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of balance proposal policy.</summary>
public static class BalanceProposalPolicy
{
    public static string BalanceProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 43605.0, in order.</summary>
    public static int BalanceProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43605.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a balance proposal code in its canonical BP- form.</summary>
    public static string NormalizeBalanceProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
