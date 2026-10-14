namespace Acme.Workforce.Contractors.Rate.RateNotice;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped rate proposal policy.</summary>
public static class CappedRateProposalPolicy
{
    /// <summary>Puts a capped rate proposal code in its canonical CRP- form.</summary>
    public static string NormalizeCappedRateProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double CappedRateProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 5192.0, in order.</summary>
    public static int CappedRateProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5192.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CappedRateProposalSpan(int start, int end)
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
}
