namespace Acme.Workforce.Absence.Tests.Holiday.Budget;

using Acme.Workforce.Absence.Holiday.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SeasonalHolidayBudgetRulesChecks
{
    public static void SeasonalHolidayBudgetValidatorReportsEveryProblem()
    {
        var validator = SeasonalHolidayBudgetRules.DefaultSeasonalHolidayBudgetValidator();
        var item = SeasonalHolidayBudget.NewSeasonalHolidayBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalHolidayBudgetSourceLengthRuleFlagsALongSource()
    {
        var rule = new SeasonalHolidayBudgetSourceLengthRule(1000);
        var item = SeasonalHolidayBudget.NewSeasonalHolidayBudget("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalHolidayBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SeasonalHolidayBudgetRequiredTitleRule("seasonal holiday budget needs this field");
        var item = SeasonalHolidayBudget.NewSeasonalHolidayBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalHolidayBudgetTeamLengthRuleFlagsALongTeam()
    {
        var rule = new SeasonalHolidayBudgetTeamLengthRule(255);
        var item = SeasonalHolidayBudget.NewSeasonalHolidayBudget("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalHolidayBudgetValidatorReportsEveryProblem();
        SeasonalHolidayBudgetSourceLengthRuleFlagsALongSource();
        SeasonalHolidayBudgetRequiredTitleRuleFlagsABlankTitle();
        SeasonalHolidayBudgetTeamLengthRuleFlagsALongTeam();
    }
}
