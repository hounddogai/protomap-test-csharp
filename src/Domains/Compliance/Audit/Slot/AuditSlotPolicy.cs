namespace Acme.Workforce.Compliance.Audit.Slot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit slot policy.</summary>
public static class AuditSlotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 15094.0, in order.</summary>
    public static int AuditSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15094.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AuditSlotSpan(int start, int end)
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
    public static int ProbeAuditSlot()
    {
        var policy = Retry.DefaultPolicy("audit-slot");
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
}
