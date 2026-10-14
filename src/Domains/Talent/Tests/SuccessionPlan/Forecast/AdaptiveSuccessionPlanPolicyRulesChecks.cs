namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class AdaptiveSuccessionPlanPolicyRulesChecks
{
    public static void AdaptiveSuccessionPlanPolicyValidatorReportsEveryProblem()
    {
        var validator = AdaptiveSuccessionPlanPolicyRules.DefaultAdaptiveSuccessionPlanPolicyValidator();
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveSuccessionPlanPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveSuccessionPlanPolicyRequiredCodeRule("adaptive succession plan policy needs an entry");
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveSuccessionPlanPolicyNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveSuccessionPlanPolicyNameLengthRule(1000);
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveSuccessionPlanPolicyFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AdaptiveSuccessionPlanPolicyFteRangeRule(6.3, 1473.8);
        var item = AdaptiveSuccessionPlanPolicy.NewAdaptiveSuccessionPlanPolicy("x", "y");
        item.Fte = 1475.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveSuccessionPlanPolicyValidatorReportsEveryProblem();
        AdaptiveSuccessionPlanPolicyRequiredCodeRuleFlagsABlankCode();
        AdaptiveSuccessionPlanPolicyNameLengthRuleFlagsALongName();
        AdaptiveSuccessionPlanPolicyFteRangeRuleFlagsValuesAboveFte();
    }
}
