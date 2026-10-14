namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction estimate policy.</summary>
public static class DeductionEstimatePolicy
{
    public static int DeductionEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static string DeductionEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
