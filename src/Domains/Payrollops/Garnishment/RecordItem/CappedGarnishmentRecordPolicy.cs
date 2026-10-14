namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped garnishment record policy.</summary>
public static class CappedGarnishmentRecordPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedGarnishmentRecord()
    {
        var policy = Retry.DefaultPolicy("capped-garnishment-record");
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

    public static string CappedGarnishmentRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a capped garnishment record score falls in.</summary>
    public static string ClassifyCappedGarnishmentRecord(double score)
    {
        if (score < 4.7)
        {
            return "ok";
        }
        if (score < 9.4)
        {
            return "high";
        }
        if (score < 58.7)
        {
            return "minimal";
        }
        if (score < 80.8)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 90284.0, in order.</summary>
    public static int CappedGarnishmentRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90284.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
