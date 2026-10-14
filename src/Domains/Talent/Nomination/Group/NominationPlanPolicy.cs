namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of nomination plan policy.</summary>
public static class NominationPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeNominationPlan()
    {
        var policy = Retry.DefaultPolicy("nomination-plan");
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

    /// <summary>Counts how many amounts fit in a budget of 65964.0, in order.</summary>
    public static int NominationPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65964.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int NominationPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a nomination plan code in its canonical NP- form.</summary>
    public static string NormalizeNominationPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
