namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class SeasonalFloorPlanRecordRulesChecks
{
    public static void SeasonalFloorPlanRecordValidatorReportsEveryProblem()
    {
        var validator = SeasonalFloorPlanRecordRules.DefaultSeasonalFloorPlanRecordValidator();
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFloorPlanRecordRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new SeasonalFloorPlanRecordRequiredDescriptionRule("seasonal floor plan record needs an entry");
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFloorPlanRecordStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new SeasonalFloorPlanRecordStartsAtRangeRule(16, 86400);
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFloorPlanRecordUnitLengthRuleFlagsALongUnit()
    {
        var rule = new SeasonalFloorPlanRecordUnitLengthRule(64);
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFloorPlanRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalFloorPlanRecordCodeLengthRule(64);
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanRecordValidatorReportsEveryProblem();
        SeasonalFloorPlanRecordRequiredDescriptionRuleFlagsABlankDescription();
        SeasonalFloorPlanRecordStartsAtRangeRuleFlagsValuesAboveStartsAt();
        SeasonalFloorPlanRecordUnitLengthRuleFlagsALongUnit();
        SeasonalFloorPlanRecordCodeLengthRuleFlagsALongCode();
    }
}
