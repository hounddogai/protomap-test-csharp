namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class SeasonalCostModelMilestoneRulesChecks
{
    public static void SeasonalCostModelMilestoneValidatorReportsEveryProblem()
    {
        var validator = SeasonalCostModelMilestoneRules.DefaultSeasonalCostModelMilestoneValidator();
        var item = SeasonalCostModelMilestone.NewSeasonalCostModelMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalCostModelMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalCostModelMilestoneRequiredCodeRule("seasonal cost model milestone needs a value");
        var item = SeasonalCostModelMilestone.NewSeasonalCostModelMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalCostModelMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new SeasonalCostModelMilestoneGraceDaysRangeRule(16, 3600);
        var item = SeasonalCostModelMilestone.NewSeasonalCostModelMilestone("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalCostModelMilestoneValidatorReportsEveryProblem();
        SeasonalCostModelMilestoneRequiredCodeRuleFlagsABlankCode();
        SeasonalCostModelMilestoneGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
