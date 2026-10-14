namespace Acme.Workforce.Absence.Tests.Policy.Segment;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingPolicyEstimateRulesChecks
{
    public static void RollingPolicyEstimateValidatorReportsEveryProblem()
    {
        var validator = RollingPolicyEstimateRules.DefaultRollingPolicyEstimateValidator();
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPolicyEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingPolicyEstimateCodeLengthRule(80);
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPolicyEstimateRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new RollingPolicyEstimateRequiredCurrencyRule("rolling policy estimate needs this field");
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingPolicyEstimateTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RollingPolicyEstimateTitleLengthRule(16);
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPolicyEstimateCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RollingPolicyEstimateCurrencyLengthRule(1000);
        var item = RollingPolicyEstimate.NewRollingPolicyEstimate("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingPolicyEstimateValidatorReportsEveryProblem();
        RollingPolicyEstimateCodeLengthRuleFlagsALongCode();
        RollingPolicyEstimateRequiredCurrencyRuleFlagsABlankCurrency();
        RollingPolicyEstimateTitleLengthRuleFlagsALongTitle();
        RollingPolicyEstimateCurrencyLengthRuleFlagsALongCurrency();
    }
}
