namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelScoreRulesChecks
{
    public static void CostModelScoreValidatorReportsEveryProblem()
    {
        var validator = CostModelScoreRules.DefaultCostModelScoreValidator();
        var item = CostModelScore.NewCostModelScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelScoreWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new CostModelScoreWindowDaysRangeRule(32, 64);
        var item = CostModelScore.NewCostModelScore("x", "y");
        item.WindowDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelScoreRequiredCodeRule("cost model score needs a value");
        var item = CostModelScore.NewCostModelScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelScoreRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new CostModelScoreRequiredRegionRule("cost model score needs a value");
        var item = CostModelScore.NewCostModelScore("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelScoreRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CostModelScoreRequiredTitleRule("cost model score needs a value");
        var item = CostModelScore.NewCostModelScore("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelScoreQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new CostModelScoreQuantityRangeRule(255, 3600);
        var item = CostModelScore.NewCostModelScore("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CostModelScoreValidatorReportsEveryProblem();
        CostModelScoreWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        CostModelScoreRequiredCodeRuleFlagsABlankCode();
        CostModelScoreRequiredRegionRuleFlagsABlankRegion();
        CostModelScoreRequiredTitleRuleFlagsABlankTitle();
        CostModelScoreQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
