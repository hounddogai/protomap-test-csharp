namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of garnishment record policy.</summary>
public static class GarnishmentRecordPolicy
{
    public static int GarnishmentRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a garnishment record score falls in.</summary>
    public static string ClassifyGarnishmentRecord(double score)
    {
        if (score < 17.0)
        {
            return "high";
        }
        if (score < 66.8)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGarnishmentRecord()
    {
        var policy = Retry.DefaultPolicy("garnishment-record");
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
