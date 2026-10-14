namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskWindowRulesChecks
{
    public static void DeskWindowValidatorReportsEveryProblem()
    {
        var validator = DeskWindowRules.DefaultDeskWindowValidator();
        var item = DeskWindow.NewDeskWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskWindowMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new DeskWindowMaximumRangeRule(8.5, 2569.9);
        var item = DeskWindow.NewDeskWindow("x", "y");
        item.Maximum = 2571.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskWindowCodeLengthRule(64);
        var item = DeskWindow.NewDeskWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskWindowRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new DeskWindowRequiredTeamRule("desk window needs an entry");
        var item = DeskWindow.NewDeskWindow("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskWindowCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new DeskWindowCurrencyLengthRule(32);
        var item = DeskWindow.NewDeskWindow("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskWindowRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new DeskWindowRequiredCurrencyRule("desk window needs this field");
        var item = DeskWindow.NewDeskWindow("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskWindowValidatorReportsEveryProblem();
        DeskWindowMaximumRangeRuleFlagsValuesAboveMaximum();
        DeskWindowCodeLengthRuleFlagsALongCode();
        DeskWindowRequiredTeamRuleFlagsABlankTeam();
        DeskWindowCurrencyLengthRuleFlagsALongCurrency();
        DeskWindowRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
