namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rate proposal policy.</summary>
public static class RateProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 70256.0, in order.</summary>
    public static int RateProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70256.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a rate proposal code in its canonical RP- form.</summary>
    public static string NormalizeRateProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RateProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRateProposal()
    {
        var policy = Retry.DefaultPolicy("rate-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
