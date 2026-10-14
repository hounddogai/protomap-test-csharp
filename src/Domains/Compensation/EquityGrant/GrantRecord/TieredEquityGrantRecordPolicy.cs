namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered equity grant record policy.</summary>
public static class TieredEquityGrantRecordPolicy
{
    /// <summary>Names the band a tiered equity grant record score falls in.</summary>
    public static string ClassifyTieredEquityGrantRecord(double score)
    {
        if (score < 28.9)
        {
            return "critical";
        }
        if (score < 57.4)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 32630.0, in order.</summary>
    public static int TieredEquityGrantRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32630.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredEquityGrantRecord()
    {
        var policy = Retry.DefaultPolicy("tiered-equity-grant-record");
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
}
