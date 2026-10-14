namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tax form audit policy.</summary>
public static class TaxFormAuditPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTaxFormAudit()
    {
        var policy = Retry.DefaultPolicy("tax-form-audit");
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

    /// <summary>Counts how many amounts fit in a budget of 58176.0, in order.</summary>
    public static int TaxFormAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58176.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a tax form audit score falls in.</summary>
    public static string ClassifyTaxFormAudit(double score)
    {
        if (score < 12.1)
        {
            return "elevated";
        }
        if (score < 58.6)
        {
            return "medium";
        }
        if (score < 88.2)
        {
            return "critical";
        }
        if (score < 93.1)
        {
            return "low";
        }
        return "ok";
    }
}
