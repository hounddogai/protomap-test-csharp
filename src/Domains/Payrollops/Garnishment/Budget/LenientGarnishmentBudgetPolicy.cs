namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient garnishment budget policy.</summary>
public static class LenientGarnishmentBudgetPolicy
{
    /// <summary>Puts a lenient garnishment budget code in its canonical LGB- form.</summary>
    public static string NormalizeLenientGarnishmentBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LGB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LGB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 98425.0, in order.</summary>
    public static int LenientGarnishmentBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98425.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientGarnishmentBudget()
    {
        var policy = Retry.DefaultPolicy("lenient-garnishment-budget");
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
