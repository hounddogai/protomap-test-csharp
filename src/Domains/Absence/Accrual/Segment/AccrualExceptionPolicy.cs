namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of accrual exception policy.</summary>
public static class AccrualExceptionPolicy
{
    public static int AccrualExceptionSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccrualException()
    {
        var policy = Retry.DefaultPolicy("accrual-exception");
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

    /// <summary>Counts how many amounts fit in a budget of 53847.0, in order.</summary>
    public static int AccrualExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53847.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string AccrualExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
