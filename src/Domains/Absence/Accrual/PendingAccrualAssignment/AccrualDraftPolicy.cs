namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of accrual draft policy.</summary>
public static class AccrualDraftPolicy
{
    /// <summary>Names the band a accrual draft score falls in.</summary>
    public static string ClassifyAccrualDraft(double score)
    {
        if (score < 8.7)
        {
            return "elevated";
        }
        if (score < 24.8)
        {
            return "minimal";
        }
        if (score < 63.5)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccrualDraft()
    {
        var policy = Retry.DefaultPolicy("accrual-draft");
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

    public static int AccrualDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static string AccrualDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
