namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive tax form ledger policy.</summary>
public static class AdaptiveTaxFormLedgerPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveTaxFormLedger()
    {
        var policy = Retry.DefaultPolicy("adaptive-tax-form-ledger");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 26651.0, in order.</summary>
    public static int AdaptiveTaxFormLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26651.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a adaptive tax form ledger score falls in.</summary>
    public static string ClassifyAdaptiveTaxFormLedger(double score)
    {
        if (score < 20.1)
        {
            return "elevated";
        }
        if (score < 68.1)
        {
            return "minimal";
        }
        return "high";
    }
}
