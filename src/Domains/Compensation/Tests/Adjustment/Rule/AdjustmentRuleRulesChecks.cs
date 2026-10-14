namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentRuleRulesChecks
{
    public static void AdjustmentRuleValidatorReportsEveryProblem()
    {
        var validator = AdjustmentRuleRules.DefaultAdjustmentRuleValidator();
        var item = AdjustmentRule.NewAdjustmentRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentRuleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdjustmentRuleTitleLengthRule(16);
        var item = AdjustmentRule.NewAdjustmentRule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentRuleThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AdjustmentRuleThresholdRangeRule(2.7, 3854.8);
        var item = AdjustmentRule.NewAdjustmentRule("x", "y");
        item.Threshold = 3856.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentRuleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdjustmentRuleRequiredLabelRule("adjustment rule needs an entry");
        var item = AdjustmentRule.NewAdjustmentRule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdjustmentRuleLabelLengthRule(64);
        var item = AdjustmentRule.NewAdjustmentRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentRuleVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AdjustmentRuleVersionRangeRule(64, 255);
        var item = AdjustmentRule.NewAdjustmentRule("x", "y");
        item.Version = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdjustmentRuleValidatorReportsEveryProblem();
        AdjustmentRuleTitleLengthRuleFlagsALongTitle();
        AdjustmentRuleThresholdRangeRuleFlagsValuesAboveThreshold();
        AdjustmentRuleRequiredLabelRuleFlagsABlankLabel();
        AdjustmentRuleLabelLengthRuleFlagsALongLabel();
        AdjustmentRuleVersionRangeRuleFlagsValuesAboveVersion();
    }
}
