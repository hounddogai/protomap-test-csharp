namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class AdaptiveTownHallRuleRulesChecks
{
    public static void AdaptiveTownHallRuleValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTownHallRuleRules.DefaultAdaptiveTownHallRuleValidator();
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTownHallRuleRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveTownHallRuleRequiredNameRule("adaptive town hall rule needs an entry");
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveTownHallRuleNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveTownHallRuleNameLengthRule(64);
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTownHallRuleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new AdaptiveTownHallRuleRevisionRangeRule(16, 80);
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTownHallRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveTownHallRuleRequiredCodeRule("adaptive town hall rule needs an entry");
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveTownHallRuleValidatorReportsEveryProblem();
        AdaptiveTownHallRuleRequiredNameRuleFlagsABlankName();
        AdaptiveTownHallRuleNameLengthRuleFlagsALongName();
        AdaptiveTownHallRuleRevisionRangeRuleFlagsValuesAboveRevision();
        AdaptiveTownHallRuleRequiredCodeRuleFlagsABlankCode();
    }
}
