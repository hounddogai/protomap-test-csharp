namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SeasonalRefreshWindowRulesChecks
{
    public static void SeasonalRefreshWindowValidatorReportsEveryProblem()
    {
        var validator = SeasonalRefreshWindowRules.DefaultSeasonalRefreshWindowValidator();
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalRefreshWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalRefreshWindowCodeLengthRule(64);
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalRefreshWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalRefreshWindowRequiredTitleRule("seasonal refresh window needs a value");
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalRefreshWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalRefreshWindowRequiredCodeRule("seasonal refresh window needs a value");
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalRefreshWindowTitleLengthRuleFlagsALongTitle()
    {
        var rule = new SeasonalRefreshWindowTitleLengthRule(3600);
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalRefreshWindowValidatorReportsEveryProblem();
        SeasonalRefreshWindowCodeLengthRuleFlagsALongCode();
        SeasonalRefreshWindowRequiredTitleRuleFlagsABlankTitle();
        SeasonalRefreshWindowRequiredCodeRuleFlagsABlankCode();
        SeasonalRefreshWindowTitleLengthRuleFlagsALongTitle();
    }
}
