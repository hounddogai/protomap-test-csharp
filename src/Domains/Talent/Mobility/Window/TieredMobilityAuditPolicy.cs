namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered mobility audit policy.</summary>
public static class TieredMobilityAuditPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredMobilityAudit()
    {
        var policy = Retry.DefaultPolicy("tiered-mobility-audit");
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

    /// <summary>Counts how many amounts fit in a budget of 35289.0, in order.</summary>
    public static int TieredMobilityAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35289.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tiered mobility audit code in its canonical TMA- form.</summary>
    public static string NormalizeTieredMobilityAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TMA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TMA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string TieredMobilityAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
