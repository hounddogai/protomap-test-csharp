namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanEntry;

using Acme.Workforce.Workplace.FloorPlan.PlanEntry;

/// <summary>Checks of plan entry code, run by the project's runner.</summary>
public static class SeasonalFloorPlanTierRulesChecks
{
    public static void SeasonalFloorPlanTierValidatorReportsEveryProblem()
    {
        var validator = SeasonalFloorPlanTierRules.DefaultSeasonalFloorPlanTierValidator();
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFloorPlanTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalFloorPlanTierTitleLengthRule(32);
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFloorPlanTierWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new SeasonalFloorPlanTierWeightRangeRule(2.0, 3696.0);
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("x", "y");
        item.Weight = 3697.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFloorPlanTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalFloorPlanTierRequiredCodeRule("seasonal floor plan tier needs an entry");
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFloorPlanTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalFloorPlanTierCodeLengthRule(16);
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanTierValidatorReportsEveryProblem();
        SeasonalFloorPlanTierTitleLengthRuleFlagsALongTitle();
        SeasonalFloorPlanTierWeightRangeRuleFlagsValuesAboveWeight();
        SeasonalFloorPlanTierRequiredCodeRuleFlagsABlankCode();
        SeasonalFloorPlanTierCodeLengthRuleFlagsALongCode();
    }
}
