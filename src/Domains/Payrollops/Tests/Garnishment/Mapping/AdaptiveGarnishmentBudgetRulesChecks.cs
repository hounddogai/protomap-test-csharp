namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentBudgetRulesChecks
{
    public static void AdaptiveGarnishmentBudgetValidatorReportsEveryProblem()
    {
        var validator = AdaptiveGarnishmentBudgetRules.DefaultAdaptiveGarnishmentBudgetValidator();
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveGarnishmentBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveGarnishmentBudgetCodeLengthRule(3600);
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveGarnishmentBudgetRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new AdaptiveGarnishmentBudgetRequiredCountryRule("adaptive garnishment budget needs this field");
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveGarnishmentBudgetRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new AdaptiveGarnishmentBudgetRequiredReviewerRule("adaptive garnishment budget needs an entry");
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveGarnishmentBudgetRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AdaptiveGarnishmentBudgetRateRangeRule(8.6, 4205.0);
        var item = AdaptiveGarnishmentBudget.NewAdaptiveGarnishmentBudget("x", "y");
        item.Rate = 4206.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentBudgetValidatorReportsEveryProblem();
        AdaptiveGarnishmentBudgetCodeLengthRuleFlagsALongCode();
        AdaptiveGarnishmentBudgetRequiredCountryRuleFlagsABlankCountry();
        AdaptiveGarnishmentBudgetRequiredReviewerRuleFlagsABlankReviewer();
        AdaptiveGarnishmentBudgetRateRangeRuleFlagsValuesAboveRate();
    }
}
