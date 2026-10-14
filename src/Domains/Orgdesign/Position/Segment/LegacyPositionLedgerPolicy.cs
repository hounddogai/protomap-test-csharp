namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy position ledger policy.</summary>
public static class LegacyPositionLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 51220.0, in order.</summary>
    public static int LegacyPositionLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51220.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string LegacyPositionLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a legacy position ledger score falls in.</summary>
    public static string ClassifyLegacyPositionLedger(double score)
    {
        if (score < 18.8)
        {
            return "ok";
        }
        if (score < 23.7)
        {
            return "minimal";
        }
        if (score < 29.4)
        {
            return "elevated";
        }
        if (score < 54.1)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyPositionLedger()
    {
        var policy = Retry.DefaultPolicy("legacy-position-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
