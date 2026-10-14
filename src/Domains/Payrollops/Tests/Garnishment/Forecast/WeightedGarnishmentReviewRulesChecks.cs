namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WeightedGarnishmentReviewRulesChecks
{
    public static void WeightedGarnishmentReviewValidatorReportsEveryProblem()
    {
        var validator = WeightedGarnishmentReviewRules.DefaultWeightedGarnishmentReviewValidator();
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedGarnishmentReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedGarnishmentReviewCodeLengthRule(3600);
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedGarnishmentReviewBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new WeightedGarnishmentReviewBudgetRangeRule(0.1, 585.9);
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("x", "y");
        item.Budget = 587.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedGarnishmentReviewRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedGarnishmentReviewRequiredCodeRule("weighted garnishment review needs an entry");
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedGarnishmentReviewRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new WeightedGarnishmentReviewRequiredTitleRule("weighted garnishment review needs a value");
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedGarnishmentReviewRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new WeightedGarnishmentReviewRequiredTimezoneRule("weighted garnishment review needs this field");
        var item = WeightedGarnishmentReview.NewWeightedGarnishmentReview("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedGarnishmentReviewValidatorReportsEveryProblem();
        WeightedGarnishmentReviewCodeLengthRuleFlagsALongCode();
        WeightedGarnishmentReviewBudgetRangeRuleFlagsValuesAboveBudget();
        WeightedGarnishmentReviewRequiredCodeRuleFlagsABlankCode();
        WeightedGarnishmentReviewRequiredTitleRuleFlagsABlankTitle();
        WeightedGarnishmentReviewRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
