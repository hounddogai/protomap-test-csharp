namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending warranty group policy.</summary>
public static class PendingWarrantyGroupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingWarrantyGroup()
    {
        var policy = Retry.DefaultPolicy("pending-warranty-group");
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

    /// <summary>Counts how many amounts fit in a budget of 7625.0, in order.</summary>
    public static int PendingWarrantyGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7625.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PendingWarrantyGroupSpan(int start, int end)
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
}
