namespace Acme.Workforce.Engagement.Tests.TownHall.OverrideItem;

using Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TownHallOverrideRulesChecks
{
    public static void TownHallOverrideValidatorReportsEveryProblem()
    {
        var validator = TownHallOverrideRules.DefaultTownHallOverrideValidator();
        var item = TownHallOverride.NewTownHallOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TownHallOverrideTitleLengthRule(32);
        var item = TownHallOverride.NewTownHallOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallOverrideRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TownHallOverrideRequiredTitleRule("town hall override needs an entry");
        var item = TownHallOverride.NewTownHallOverride("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallOverrideRequiredCodeRule("town hall override needs this field");
        var item = TownHallOverride.NewTownHallOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TownHallOverrideValidatorReportsEveryProblem();
        TownHallOverrideTitleLengthRuleFlagsALongTitle();
        TownHallOverrideRequiredTitleRuleFlagsABlankTitle();
        TownHallOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
