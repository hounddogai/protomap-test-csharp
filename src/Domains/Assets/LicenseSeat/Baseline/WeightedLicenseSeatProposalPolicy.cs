namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Functions of weighted license seat proposal policy.</summary>
public static class WeightedLicenseSeatProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 81380.0, in order.</summary>
    public static int WeightedLicenseSeatProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81380.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WeightedLicenseSeatProposalSpan(int start, int end)
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

    /// <summary>Puts a weighted license seat proposal code in its canonical WLSP- form.</summary>
    public static string NormalizeWeightedLicenseSeatProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WLSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WLSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
