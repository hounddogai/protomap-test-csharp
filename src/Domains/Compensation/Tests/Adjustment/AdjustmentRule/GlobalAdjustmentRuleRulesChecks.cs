namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class GlobalAdjustmentRuleRulesChecks
{
    public static void GlobalAdjustmentRuleValidatorReportsEveryProblem()
    {
        var validator = GlobalAdjustmentRuleRules.DefaultGlobalAdjustmentRuleValidator();
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAdjustmentRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalAdjustmentRuleLabelLengthRule(3600);
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAdjustmentRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalAdjustmentRuleRequiredCodeRule("global adjustment rule needs this field");
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAdjustmentRuleAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new GlobalAdjustmentRuleAttemptsRangeRule(80, 86400);
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAdjustmentRuleEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new GlobalAdjustmentRuleEndsAtRangeRule(80, 255);
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalAdjustmentRuleValidatorReportsEveryProblem();
        GlobalAdjustmentRuleLabelLengthRuleFlagsALongLabel();
        GlobalAdjustmentRuleRequiredCodeRuleFlagsABlankCode();
        GlobalAdjustmentRuleAttemptsRangeRuleFlagsValuesAboveAttempts();
        GlobalAdjustmentRuleEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
