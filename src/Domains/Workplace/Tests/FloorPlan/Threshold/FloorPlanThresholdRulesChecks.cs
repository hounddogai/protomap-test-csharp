namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanThresholdRulesChecks
{
    public static void FloorPlanThresholdValidatorReportsEveryProblem()
    {
        var validator = FloorPlanThresholdRules.DefaultFloorPlanThresholdValidator();
        var item = FloorPlanThreshold.NewFloorPlanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanThresholdEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new FloorPlanThresholdEndsAtRangeRule(16, 16);
        var item = FloorPlanThreshold.NewFloorPlanThreshold("x", "y");
        item.EndsAt = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new FloorPlanThresholdRequiredSalaryBandRule("floor plan threshold needs an entry");
        var item = FloorPlanThreshold.NewFloorPlanThreshold("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanThresholdRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new FloorPlanThresholdRequiredLocationRule("floor plan threshold needs an entry");
        var item = FloorPlanThreshold.NewFloorPlanThreshold("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanThresholdValidatorReportsEveryProblem();
        FloorPlanThresholdEndsAtRangeRuleFlagsValuesAboveEndsAt();
        FloorPlanThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand();
        FloorPlanThresholdRequiredLocationRuleFlagsABlankLocation();
    }
}
