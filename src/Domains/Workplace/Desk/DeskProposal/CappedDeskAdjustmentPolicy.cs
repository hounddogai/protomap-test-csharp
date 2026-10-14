namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped desk adjustment policy.</summary>
public static class CappedDeskAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 43652.0, in order.</summary>
    public static int CappedDeskAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43652.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double CappedDeskAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedDeskAdjustment()
    {
        var policy = Retry.DefaultPolicy("capped-desk-adjustment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a capped desk adjustment code in its canonical CDA- form.</summary>
    public static string NormalizeCappedDeskAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CDA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CDA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
