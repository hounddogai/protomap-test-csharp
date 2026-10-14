namespace Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Functions of deduction record policy.</summary>
public static class DeductionRecordPolicy
{
    /// <summary>Names the band a deduction record score falls in.</summary>
    public static string ClassifyDeductionRecord(double score)
    {
        if (score < 14.3)
        {
            return "high";
        }
        if (score < 39.9)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 29130.0, in order.</summary>
    public static int DeductionRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29130.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
