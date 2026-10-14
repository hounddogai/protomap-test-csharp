namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy tax form exception policy.</summary>
public static class LegacyTaxFormExceptionPolicy
{
    /// <summary>Puts a legacy tax form exception code in its canonical LTFE- form.</summary>
    public static string NormalizeLegacyTaxFormExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LTFE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LTFE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyTaxFormException()
    {
        var policy = Retry.DefaultPolicy("legacy-tax-form-exception");
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

    /// <summary>Counts how many amounts fit in a budget of 41046.0, in order.</summary>
    public static int LegacyTaxFormExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41046.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LegacyTaxFormExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}
