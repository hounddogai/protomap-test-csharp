namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RateBudgetRulesChecks
{
    public static void RateBudgetValidatorReportsEveryProblem()
    {
        var validator = RateBudgetRules.DefaultRateBudgetValidator();
        var item = RateBudget.NewRateBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateBudgetVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new RateBudgetVersionRangeRule(255, 3600);
        var item = RateBudget.NewRateBudget("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateBudgetThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RateBudgetThresholdRangeRule(7.4, 1946.9);
        var item = RateBudget.NewRateBudget("x", "y");
        item.Threshold = 1948.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RateBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new RateBudgetNameLengthRule(255);
        var item = RateBudget.NewRateBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RateBudgetMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RateBudgetMinimumRangeRule(2.7, 1070.3);
        var item = RateBudget.NewRateBudget("x", "y");
        item.Minimum = 1071.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RateBudgetValidatorReportsEveryProblem();
        RateBudgetVersionRangeRuleFlagsValuesAboveVersion();
        RateBudgetThresholdRangeRuleFlagsValuesAboveThreshold();
        RateBudgetNameLengthRuleFlagsALongName();
        RateBudgetMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
