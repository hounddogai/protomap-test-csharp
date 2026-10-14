namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of merit cycle draft policy.</summary>
public static class MeritCycleDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 88536.0, in order.</summary>
    public static int MeritCycleDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88536.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMeritCycleDraft()
    {
        var policy = Retry.DefaultPolicy("merit-cycle-draft");
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

    /// <summary>Puts a merit cycle draft code in its canonical MCD- form.</summary>
    public static string NormalizeMeritCycleDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
