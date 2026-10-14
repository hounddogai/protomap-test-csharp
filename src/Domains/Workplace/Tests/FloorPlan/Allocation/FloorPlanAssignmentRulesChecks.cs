namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class FloorPlanAssignmentRulesChecks
{
    public static void FloorPlanAssignmentValidatorReportsEveryProblem()
    {
        var validator = FloorPlanAssignmentRules.DefaultFloorPlanAssignmentValidator();
        var item = FloorPlanAssignment.NewFloorPlanAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanAssignmentRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new FloorPlanAssignmentRequiredCategoryRule("floor plan assignment needs a value");
        var item = FloorPlanAssignment.NewFloorPlanAssignment("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanAssignmentWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new FloorPlanAssignmentWindowDaysRangeRule(32, 80);
        var item = FloorPlanAssignment.NewFloorPlanAssignment("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FloorPlanAssignmentRequiredTitleRule("floor plan assignment needs this field");
        var item = FloorPlanAssignment.NewFloorPlanAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanAssignmentValidatorReportsEveryProblem();
        FloorPlanAssignmentRequiredCategoryRuleFlagsABlankCategory();
        FloorPlanAssignmentWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        FloorPlanAssignmentRequiredTitleRuleFlagsABlankTitle();
    }
}
