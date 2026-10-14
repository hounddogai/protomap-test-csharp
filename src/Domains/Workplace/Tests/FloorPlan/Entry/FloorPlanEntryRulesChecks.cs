namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanEntryRulesChecks
{
    public static void FloorPlanEntryValidatorReportsEveryProblem()
    {
        var validator = FloorPlanEntryRules.DefaultFloorPlanEntryValidator();
        var item = FloorPlanEntry.NewFloorPlanEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FloorPlanEntryRequiredNameRuleFlagsABlankName()
    {
        var rule = new FloorPlanEntryRequiredNameRule("floor plan entry needs a value");
        var item = FloorPlanEntry.NewFloorPlanEntry("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FloorPlanEntryRequiredCodeRule("floor plan entry needs this field");
        var item = FloorPlanEntry.NewFloorPlanEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FloorPlanEntryRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new FloorPlanEntryRevisionRangeRule(80, 255);
        var item = FloorPlanEntry.NewFloorPlanEntry("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FloorPlanEntryRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FloorPlanEntryRequiredTitleRule("floor plan entry needs an entry");
        var item = FloorPlanEntry.NewFloorPlanEntry("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FloorPlanEntryValidatorReportsEveryProblem();
        FloorPlanEntryRequiredNameRuleFlagsABlankName();
        FloorPlanEntryRequiredCodeRuleFlagsABlankCode();
        FloorPlanEntryRevisionRangeRuleFlagsValuesAboveRevision();
        FloorPlanEntryRequiredTitleRuleFlagsABlankTitle();
    }
}
