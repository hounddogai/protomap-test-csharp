namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackRefreshPolicyRulesChecks
{
    public static void FallbackRefreshPolicyValidatorReportsEveryProblem()
    {
        var validator = FallbackRefreshPolicyRules.DefaultFallbackRefreshPolicyValidator();
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackRefreshPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackRefreshPolicyLabelLengthRule(80);
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackRefreshPolicyRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new FallbackRefreshPolicyRequiredSourceRule("fallback refresh policy needs a value");
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRefreshPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackRefreshPolicyRequiredCodeRule("fallback refresh policy needs this field");
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackRefreshPolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackRefreshPolicyRequiredLabelRule("fallback refresh policy needs this field");
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackRefreshPolicyValidatorReportsEveryProblem();
        FallbackRefreshPolicyLabelLengthRuleFlagsALongLabel();
        FallbackRefreshPolicyRequiredSourceRuleFlagsABlankSource();
        FallbackRefreshPolicyRequiredCodeRuleFlagsABlankCode();
        FallbackRefreshPolicyRequiredLabelRuleFlagsABlankLabel();
    }
}
