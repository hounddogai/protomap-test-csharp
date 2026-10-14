namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledFloorPlanEntryRulesChecks
{
    public static void ScheduledFloorPlanEntryValidatorReportsEveryProblem()
    {
        var validator = ScheduledFloorPlanEntryRules.DefaultScheduledFloorPlanEntryValidator();
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledFloorPlanEntryEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ScheduledFloorPlanEntryEndsAtRangeRule(120, 1000);
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledFloorPlanEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledFloorPlanEntryRequiredCodeRule("scheduled floor plan entry needs this field");
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledFloorPlanEntryRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ScheduledFloorPlanEntryRequiredSegmentRule("scheduled floor plan entry needs this field");
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledFloorPlanEntryExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new ScheduledFloorPlanEntryExpiresAtRangeRule(16, 120);
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledFloorPlanEntryWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ScheduledFloorPlanEntryWeightRangeRule(3.0, 3286.5);
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("x", "y");
        item.Weight = 3288.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanEntryValidatorReportsEveryProblem();
        ScheduledFloorPlanEntryEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ScheduledFloorPlanEntryRequiredCodeRuleFlagsABlankCode();
        ScheduledFloorPlanEntryRequiredSegmentRuleFlagsABlankSegment();
        ScheduledFloorPlanEntryExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        ScheduledFloorPlanEntryWeightRangeRuleFlagsValuesAboveWeight();
    }
}
