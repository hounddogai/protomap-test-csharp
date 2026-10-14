namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of merit cycle policy policy.</summary>
public static class MeritCyclePolicyPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMeritCyclePolicy()
    {
        var policy = Retry.DefaultPolicy("merit-cycle-policy");
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

    /// <summary>Counts how many amounts fit in a budget of 12001.0, in order.</summary>
    public static int MeritCyclePolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12001.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a merit cycle policy code in its canonical MCP- form.</summary>
    public static string NormalizeMeritCyclePolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
