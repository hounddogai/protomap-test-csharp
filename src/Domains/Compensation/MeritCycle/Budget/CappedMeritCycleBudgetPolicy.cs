namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped merit cycle budget policy.</summary>
public static class CappedMeritCycleBudgetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedMeritCycleBudget()
    {
        var policy = Retry.DefaultPolicy("capped-merit-cycle-budget");
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

    /// <summary>Puts a capped merit cycle budget code in its canonical CMCB- form.</summary>
    public static string NormalizeCappedMeritCycleBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMCB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMCB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
