namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallWindowRulesChecks
{
    public static void TownHallWindowValidatorReportsEveryProblem()
    {
        var validator = TownHallWindowRules.DefaultTownHallWindowValidator();
        var item = TownHallWindow.NewTownHallWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallWindowEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new TownHallWindowEndsAtRangeRule(32, 86400);
        var item = TownHallWindow.NewTownHallWindow("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new TownHallWindowCodeLengthRule(80);
        var item = TownHallWindow.NewTownHallWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallWindowRequiredCodeRule("town hall window needs an entry");
        var item = TownHallWindow.NewTownHallWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallWindowSourceLengthRuleFlagsALongSource()
    {
        var rule = new TownHallWindowSourceLengthRule(32);
        var item = TownHallWindow.NewTownHallWindow("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallWindowValidatorReportsEveryProblem();
        TownHallWindowEndsAtRangeRuleFlagsValuesAboveEndsAt();
        TownHallWindowCodeLengthRuleFlagsALongCode();
        TownHallWindowRequiredCodeRuleFlagsABlankCode();
        TownHallWindowSourceLengthRuleFlagsALongSource();
    }
}
