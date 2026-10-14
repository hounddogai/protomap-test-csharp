namespace Acme.Workforce.Payrollops.Payslip.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped payslip reservation policy.</summary>
public static class CappedPayslipReservationPolicy
{
    /// <summary>Names the band a capped payslip reservation score falls in.</summary>
    public static string ClassifyCappedPayslipReservation(double score)
    {
        if (score < 6.5)
        {
            return "high";
        }
        if (score < 35.9)
        {
            return "medium";
        }
        if (score < 66.6)
        {
            return "critical";
        }
        if (score < 70.6)
        {
            return "low";
        }
        return "ok";
    }

    public static string CappedPayslipReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
