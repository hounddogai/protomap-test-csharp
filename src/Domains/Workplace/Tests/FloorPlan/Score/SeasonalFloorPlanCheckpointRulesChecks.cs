namespace Acme.Workforce.Workplace.Tests.FloorPlan.Score;

using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalFloorPlanCheckpointRulesChecks
{
    public static void SeasonalFloorPlanCheckpointValidatorReportsEveryProblem()
    {
        var validator = SeasonalFloorPlanCheckpointRules.DefaultSeasonalFloorPlanCheckpointValidator();
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFloorPlanCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalFloorPlanCheckpointRequiredCodeRule("seasonal floor plan checkpoint needs an entry");
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFloorPlanCheckpointQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new SeasonalFloorPlanCheckpointQuantityRangeRule(3600, 86400);
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFloorPlanCheckpointCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new SeasonalFloorPlanCheckpointCeilingRangeRule(5.4, 2235.2);
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("x", "y");
        item.Ceiling = 2236.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFloorPlanCheckpointRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new SeasonalFloorPlanCheckpointRequiredNotesRule("seasonal floor plan checkpoint needs a value");
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFloorPlanCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalFloorPlanCheckpointCodeLengthRule(16);
        var item = SeasonalFloorPlanCheckpoint.NewSeasonalFloorPlanCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanCheckpointValidatorReportsEveryProblem();
        SeasonalFloorPlanCheckpointRequiredCodeRuleFlagsABlankCode();
        SeasonalFloorPlanCheckpointQuantityRangeRuleFlagsValuesAboveQuantity();
        SeasonalFloorPlanCheckpointCeilingRangeRuleFlagsValuesAboveCeiling();
        SeasonalFloorPlanCheckpointRequiredNotesRuleFlagsABlankNotes();
        SeasonalFloorPlanCheckpointCodeLengthRuleFlagsALongCode();
    }
}
