namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of reorg audit policy.</summary>
public static class ReorgAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 66302.0, in order.</summary>
    public static int ReorgAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66302.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string ReorgAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeReorgAudit()
    {
        var policy = Retry.DefaultPolicy("reorg-audit");
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

    /// <summary>Puts a reorg audit code in its canonical RA- form.</summary>
    public static string NormalizeReorgAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
