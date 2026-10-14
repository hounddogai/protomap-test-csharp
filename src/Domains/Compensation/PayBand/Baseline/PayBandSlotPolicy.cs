namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band slot policy.</summary>
public static class PayBandSlotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandSlot()
    {
        var policy = Retry.DefaultPolicy("pay-band-slot");
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

    public static int PayBandSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 61761.0, in order.</summary>
    public static int PayBandSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61761.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
