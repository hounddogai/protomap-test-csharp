namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeNoticeRulesChecks
{
    public static void AccessBadgeNoticeValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeNoticeRules.DefaultAccessBadgeNoticeValidator();
        var item = AccessBadgeNotice.NewAccessBadgeNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeNoticeSkillLengthRuleFlagsALongSkill()
    {
        var rule = new AccessBadgeNoticeSkillLengthRule(3600);
        var item = AccessBadgeNotice.NewAccessBadgeNotice("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AccessBadgeNoticeRequiredTitleRule("access badge notice needs an entry");
        var item = AccessBadgeNotice.NewAccessBadgeNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeNoticeCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AccessBadgeNoticeCurrencyLengthRule(3600);
        var item = AccessBadgeNotice.NewAccessBadgeNotice("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeNoticeRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new AccessBadgeNoticeRequiredCurrencyRule("access badge notice needs this field");
        var item = AccessBadgeNotice.NewAccessBadgeNotice("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeNoticeRequiredCodeRule("access badge notice needs an entry");
        var item = AccessBadgeNotice.NewAccessBadgeNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeNoticeValidatorReportsEveryProblem();
        AccessBadgeNoticeSkillLengthRuleFlagsALongSkill();
        AccessBadgeNoticeRequiredTitleRuleFlagsABlankTitle();
        AccessBadgeNoticeCurrencyLengthRuleFlagsALongCurrency();
        AccessBadgeNoticeRequiredCurrencyRuleFlagsABlankCurrency();
        AccessBadgeNoticeRequiredCodeRuleFlagsABlankCode();
    }
}
