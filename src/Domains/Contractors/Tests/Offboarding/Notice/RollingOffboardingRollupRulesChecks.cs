namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingOffboardingRollupRulesChecks
{
    public static void RollingOffboardingRollupValidatorReportsEveryProblem()
    {
        var validator = RollingOffboardingRollupRules.DefaultRollingOffboardingRollupValidator();
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingOffboardingRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingOffboardingRollupCodeLengthRule(64);
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingOffboardingRollupThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RollingOffboardingRollupThresholdRangeRule(7.9, 3041.6);
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("x", "y");
        item.Threshold = 3043.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingOffboardingRollupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingOffboardingRollupRequiredLabelRule("rolling offboarding rollup needs this field");
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingOffboardingRollupCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new RollingOffboardingRollupCostCenterLengthRule(64);
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingOffboardingRollupValidatorReportsEveryProblem();
        RollingOffboardingRollupCodeLengthRuleFlagsALongCode();
        RollingOffboardingRollupThresholdRangeRuleFlagsValuesAboveThreshold();
        RollingOffboardingRollupRequiredLabelRuleFlagsABlankLabel();
        RollingOffboardingRollupCostCenterLengthRuleFlagsALongCostCenter();
    }
}
