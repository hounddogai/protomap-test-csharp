namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack ledger policy.</summary>
public static class PolicyAckLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 12962.0, in order.</summary>
    public static int PolicyAckLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12962.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a policy ack ledger code in its canonical PAL- form.</summary>
    public static string NormalizePolicyAckLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PolicyAckLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
