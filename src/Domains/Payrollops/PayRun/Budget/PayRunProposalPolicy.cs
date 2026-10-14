namespace Acme.Workforce.Payrollops.PayRun.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay run proposal policy.</summary>
public static class PayRunProposalPolicy
{
    public static string PayRunProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 13728.0, in order.</summary>
    public static int PayRunProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13728.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a pay run proposal code in its canonical PRP- form.</summary>
    public static string NormalizePayRunProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
