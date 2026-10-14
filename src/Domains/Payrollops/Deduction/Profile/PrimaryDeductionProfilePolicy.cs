namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary deduction profile policy.</summary>
public static class PrimaryDeductionProfilePolicy
{
    public static double PrimaryDeductionProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string PrimaryDeductionProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
