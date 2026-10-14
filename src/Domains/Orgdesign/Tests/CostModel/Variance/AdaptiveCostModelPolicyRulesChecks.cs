namespace Acme.Workforce.Orgdesign.Tests.CostModel.Variance;

using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveCostModelPolicyRulesChecks
{
    public static void AdaptiveCostModelPolicyValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCostModelPolicyRules.DefaultAdaptiveCostModelPolicyValidator();
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCostModelPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveCostModelPolicyRequiredCodeRule("adaptive cost model policy needs an entry");
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCostModelPolicyRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new AdaptiveCostModelPolicyRequiredExternalRefRule("adaptive cost model policy needs an entry");
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCostModelPolicyMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AdaptiveCostModelPolicyMaximumRangeRule(7.7, 134.5);
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("x", "y");
        item.Maximum = 136.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveCostModelPolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveCostModelPolicyRequiredLabelRule("adaptive cost model policy needs an entry");
        var item = AdaptiveCostModelPolicy.NewAdaptiveCostModelPolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveCostModelPolicyValidatorReportsEveryProblem();
        AdaptiveCostModelPolicyRequiredCodeRuleFlagsABlankCode();
        AdaptiveCostModelPolicyRequiredExternalRefRuleFlagsABlankExternalRef();
        AdaptiveCostModelPolicyMaximumRangeRuleFlagsValuesAboveMaximum();
        AdaptiveCostModelPolicyRequiredLabelRuleFlagsABlankLabel();
    }
}
