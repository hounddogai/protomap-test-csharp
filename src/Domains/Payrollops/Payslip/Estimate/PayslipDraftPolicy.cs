namespace Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Functions of payslip draft policy.</summary>
public static class PayslipDraftPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 39410.0, in order.</summary>
    public static int PayslipDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39410.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a payslip draft score falls in.</summary>
    public static string ClassifyPayslipDraft(double score)
    {
        if (score < 18.4)
        {
            return "elevated";
        }
        if (score < 24.9)
        {
            return "minimal";
        }
        if (score < 49.7)
        {
            return "low";
        }
        if (score < 58.8)
        {
            return "high";
        }
        return "ok";
    }
}
