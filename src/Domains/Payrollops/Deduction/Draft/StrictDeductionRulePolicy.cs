namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict deduction rule policy.</summary>
public static class StrictDeductionRulePolicy
{
    public static string StrictDeductionRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double StrictDeductionRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
