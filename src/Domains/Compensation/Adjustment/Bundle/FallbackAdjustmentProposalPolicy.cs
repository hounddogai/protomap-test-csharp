namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback adjustment proposal policy.</summary>
public static class FallbackAdjustmentProposalPolicy
{
    /// <summary>Puts a fallback adjustment proposal code in its canonical FAP- form.</summary>
    public static string NormalizeFallbackAdjustmentProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FallbackAdjustmentProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackAdjustmentProposal()
    {
        var policy = Retry.DefaultPolicy("fallback-adjustment-proposal");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 85728.0, in order.</summary>
    public static int FallbackAdjustmentProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85728.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
