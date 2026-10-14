namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of desk quota policy.</summary>
public static class DeskQuotaPolicy
{
    public static int DeskQuotaSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDeskQuota()
    {
        var policy = Retry.DefaultPolicy("desk-quota");
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

    /// <summary>Names the band a desk quota score falls in.</summary>
    public static string ClassifyDeskQuota(double score)
    {
        if (score < 18.7)
        {
            return "elevated";
        }
        if (score < 46.1)
        {
            return "medium";
        }
        if (score < 83.4)
        {
            return "low";
        }
        if (score < 93.2)
        {
            return "watch";
        }
        return "high";
    }
}
