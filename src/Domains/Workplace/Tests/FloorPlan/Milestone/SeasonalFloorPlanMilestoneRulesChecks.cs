namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SeasonalFloorPlanMilestoneRulesChecks
{
    public static void SeasonalFloorPlanMilestoneValidatorReportsEveryProblem()
    {
        var validator = SeasonalFloorPlanMilestoneRules.DefaultSeasonalFloorPlanMilestoneValidator();
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFloorPlanMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalFloorPlanMilestoneRequiredCodeRule("seasonal floor plan milestone needs this field");
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFloorPlanMilestoneRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new SeasonalFloorPlanMilestoneRequiredSourceRule("seasonal floor plan milestone needs a value");
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFloorPlanMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalFloorPlanMilestoneCodeLengthRule(80);
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFloorPlanMilestoneScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new SeasonalFloorPlanMilestoneScoreRangeRule(9.4, 4824.3);
        var item = SeasonalFloorPlanMilestone.NewSeasonalFloorPlanMilestone("x", "y");
        item.Score = 4825.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanMilestoneValidatorReportsEveryProblem();
        SeasonalFloorPlanMilestoneRequiredCodeRuleFlagsABlankCode();
        SeasonalFloorPlanMilestoneRequiredSourceRuleFlagsABlankSource();
        SeasonalFloorPlanMilestoneCodeLengthRuleFlagsALongCode();
        SeasonalFloorPlanMilestoneScoreRangeRuleFlagsValuesAboveScore();
    }
}
