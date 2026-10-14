namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary statement of work window policy.</summary>
public static class PrimaryStatementOfWorkWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 93854.0, in order.</summary>
    public static int PrimaryStatementOfWorkWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93854.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryStatementOfWorkWindow()
    {
        var policy = Retry.DefaultPolicy("primary-statement-of-work-window");
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

    /// <summary>Puts a primary statement of work window code in its canonical PSOWW- form.</summary>
    public static string NormalizePrimaryStatementOfWorkWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSOWW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSOWW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
