namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of visitor request policy.</summary>
public static class VisitorRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorRequest()
    {
        var policy = Retry.DefaultPolicy("visitor-request");
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

    /// <summary>Counts how many amounts fit in a budget of 57183.0, in order.</summary>
    public static int VisitorRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57183.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
