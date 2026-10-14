namespace Acme.Workforce.Payrollops.Payslip.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of payslip schedule policy.</summary>
public static class PayslipSchedulePolicy
{
    public static double PayslipScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PayslipScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
