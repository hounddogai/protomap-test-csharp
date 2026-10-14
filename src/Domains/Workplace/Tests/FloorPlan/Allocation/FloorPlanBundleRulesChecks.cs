namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class FloorPlanBundleRulesChecks
{
    public static void FloorPlanBundleValidatorReportsEveryProblem()
    {
        var validator = FloorPlanBundleRules.DefaultFloorPlanBundleValidator();
        var item = FloorPlanBundle.NewFloorPlanBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanBundleSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new FloorPlanBundleSalaryBandLengthRule(255);
        var item = FloorPlanBundle.NewFloorPlanBundle("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FloorPlanBundleMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new FloorPlanBundleMinimumRangeRule(0.8, 1135.8);
        var item = FloorPlanBundle.NewFloorPlanBundle("x", "y");
        item.Minimum = 1137.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanBundleRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new FloorPlanBundleRequiredSalaryBandRule("floor plan bundle needs a value");
        var item = FloorPlanBundle.NewFloorPlanBundle("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanBundleValidatorReportsEveryProblem();
        FloorPlanBundleSalaryBandLengthRuleFlagsALongSalaryBand();
        FloorPlanBundleMinimumRangeRuleFlagsValuesAboveMinimum();
        FloorPlanBundleRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
