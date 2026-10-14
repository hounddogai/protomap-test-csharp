namespace Acme.Workforce.Absence.Policy.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback policy budget policy.</summary>
public static class FallbackPolicyBudgetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackPolicyBudget()
    {
        var policy = Retry.DefaultPolicy("fallback-policy-budget");
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

    public static int FallbackPolicyBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
