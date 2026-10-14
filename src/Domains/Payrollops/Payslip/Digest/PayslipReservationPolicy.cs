namespace Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Functions of payslip reservation policy.</summary>
public static class PayslipReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 61760.0, in order.</summary>
    public static int PayslipReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61760.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a payslip reservation score falls in.</summary>
    public static string ClassifyPayslipReservation(double score)
    {
        if (score < 15.2)
        {
            return "watch";
        }
        if (score < 28.8)
        {
            return "critical";
        }
        if (score < 52.4)
        {
            return "medium";
        }
        return "minimal";
    }
}
