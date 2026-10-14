namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary return plan manifest policy.</summary>
public static class PrimaryReturnPlanManifestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryReturnPlanManifest()
    {
        var policy = Retry.DefaultPolicy("primary-return-plan-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 17380.0, in order.</summary>
    public static int PrimaryReturnPlanManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17380.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
