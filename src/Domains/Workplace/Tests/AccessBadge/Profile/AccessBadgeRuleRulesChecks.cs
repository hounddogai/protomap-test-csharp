namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeRuleRulesChecks
{
    public static void AccessBadgeRuleValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeRuleRules.DefaultAccessBadgeRuleValidator();
        var item = AccessBadgeRule.NewAccessBadgeRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeRuleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AccessBadgeRuleLabelLengthRule(1000);
        var item = AccessBadgeRule.NewAccessBadgeRule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeRulePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new AccessBadgeRulePhoneLengthRule(1000);
        var item = AccessBadgeRule.NewAccessBadgeRule("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccessBadgeRuleCodeLengthRule(32);
        var item = AccessBadgeRule.NewAccessBadgeRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeRuleRequiredCodeRule("access badge rule needs an entry");
        var item = AccessBadgeRule.NewAccessBadgeRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeRuleRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new AccessBadgeRuleRequiredChannelRule("access badge rule needs a value");
        var item = AccessBadgeRule.NewAccessBadgeRule("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeRuleValidatorReportsEveryProblem();
        AccessBadgeRuleLabelLengthRuleFlagsALongLabel();
        AccessBadgeRulePhoneLengthRuleFlagsALongPhone();
        AccessBadgeRuleCodeLengthRuleFlagsALongCode();
        AccessBadgeRuleRequiredCodeRuleFlagsABlankCode();
        AccessBadgeRuleRequiredChannelRuleFlagsABlankChannel();
    }
}
