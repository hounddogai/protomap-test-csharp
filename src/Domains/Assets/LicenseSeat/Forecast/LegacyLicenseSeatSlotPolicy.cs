namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy license seat slot policy.</summary>
public static class LegacyLicenseSeatSlotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 60944.0, in order.</summary>
    public static int LegacyLicenseSeatSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60944.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyLicenseSeatSlot()
    {
        var policy = Retry.DefaultPolicy("legacy-license-seat-slot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int LegacyLicenseSeatSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
