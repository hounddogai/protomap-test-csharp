namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanBudgetRulesChecks
{
    public static void SeasonalSuccessionPlanBudgetValidatorReportsEveryProblem()
    {
        var validator = SeasonalSuccessionPlanBudgetRules.DefaultSeasonalSuccessionPlanBudgetValidator();
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalSuccessionPlanBudgetSourceLengthRuleFlagsALongSource()
    {
        var rule = new SeasonalSuccessionPlanBudgetSourceLengthRule(32);
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSuccessionPlanBudgetRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new SeasonalSuccessionPlanBudgetRatingRangeRule(8.2, 1145.5);
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("x", "y");
        item.Rating = 1147.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalSuccessionPlanBudgetCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new SeasonalSuccessionPlanBudgetCurrencyLengthRule(64);
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalSuccessionPlanBudgetRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new SeasonalSuccessionPlanBudgetRatioRangeRule(6.8, 351.1);
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("x", "y");
        item.Ratio = 352.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanBudgetValidatorReportsEveryProblem();
        SeasonalSuccessionPlanBudgetSourceLengthRuleFlagsALongSource();
        SeasonalSuccessionPlanBudgetRatingRangeRuleFlagsValuesAboveRating();
        SeasonalSuccessionPlanBudgetCurrencyLengthRuleFlagsALongCurrency();
        SeasonalSuccessionPlanBudgetRatioRangeRuleFlagsValuesAboveRatio();
    }
}
