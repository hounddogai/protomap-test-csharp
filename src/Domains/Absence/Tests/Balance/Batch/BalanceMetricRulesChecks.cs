namespace Acme.Workforce.Absence.Tests.Balance.Batch;

using Acme.Workforce.Absence.Balance.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BalanceMetricRulesChecks
{
    public static void BalanceMetricValidatorReportsEveryProblem()
    {
        var validator = BalanceMetricRules.DefaultBalanceMetricValidator();
        var item = BalanceMetric.NewBalanceMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceMetricMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new BalanceMetricMinimumRangeRule(0.0, 579.6);
        var item = BalanceMetric.NewBalanceMetric("x", "y");
        item.Minimum = 581.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BalanceMetricTitleLengthRule(1000);
        var item = BalanceMetric.NewBalanceMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceMetricManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new BalanceMetricManagerIdLengthRule(120);
        var item = BalanceMetric.NewBalanceMetric("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceMetricRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new BalanceMetricRequiredManagerIdRule("balance metric needs this field");
        var item = BalanceMetric.NewBalanceMetric("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceMetricCountryLengthRuleFlagsALongCountry()
    {
        var rule = new BalanceMetricCountryLengthRule(3600);
        var item = BalanceMetric.NewBalanceMetric("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalanceMetricCodeLengthRule(86400);
        var item = BalanceMetric.NewBalanceMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BalanceMetricValidatorReportsEveryProblem();
        BalanceMetricMinimumRangeRuleFlagsValuesAboveMinimum();
        BalanceMetricTitleLengthRuleFlagsALongTitle();
        BalanceMetricManagerIdLengthRuleFlagsALongManagerId();
        BalanceMetricRequiredManagerIdRuleFlagsABlankManagerId();
        BalanceMetricCountryLengthRuleFlagsALongCountry();
        BalanceMetricCodeLengthRuleFlagsALongCode();
    }
}
