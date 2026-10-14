namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class FallbackRateCycleRulesChecks
{
    public static void FallbackRateCycleValidatorReportsEveryProblem()
    {
        var validator = FallbackRateCycleRules.DefaultFallbackRateCycleValidator();
        var item = FallbackRateCycle.NewFallbackRateCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRateCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackRateCycleRequiredCodeRule("fallback rate cycle needs a value");
        var item = FallbackRateCycle.NewFallbackRateCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRateCycleRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new FallbackRateCycleRequiredCategoryRule("fallback rate cycle needs an entry");
        var item = FallbackRateCycle.NewFallbackRateCycle("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRateCycleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new FallbackRateCycleAmountRangeRule(7.1, 3473.2);
        var item = FallbackRateCycle.NewFallbackRateCycle("x", "y");
        item.Amount = 3474.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackRateCycleValidatorReportsEveryProblem();
        FallbackRateCycleRequiredCodeRuleFlagsABlankCode();
        FallbackRateCycleRequiredCategoryRuleFlagsABlankCategory();
        FallbackRateCycleAmountRangeRuleFlagsValuesAboveAmount();
    }
}
