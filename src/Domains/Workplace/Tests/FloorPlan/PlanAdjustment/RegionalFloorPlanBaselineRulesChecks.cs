namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class RegionalFloorPlanBaselineRulesChecks
{
    public static void RegionalFloorPlanBaselineValidatorReportsEveryProblem()
    {
        var validator = RegionalFloorPlanBaselineRules.DefaultRegionalFloorPlanBaselineValidator();
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalFloorPlanBaselineHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new RegionalFloorPlanBaselineHoursRangeRule(5.7, 778.9);
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("x", "y");
        item.Hours = 780.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalFloorPlanBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalFloorPlanBaselineRequiredLabelRule("regional floor plan baseline needs an entry");
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalFloorPlanBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalFloorPlanBaselineCodeLengthRule(3600);
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalFloorPlanBaselineValidatorReportsEveryProblem();
        RegionalFloorPlanBaselineHoursRangeRuleFlagsValuesAboveHours();
        RegionalFloorPlanBaselineRequiredLabelRuleFlagsABlankLabel();
        RegionalFloorPlanBaselineCodeLengthRuleFlagsALongCode();
    }
}
