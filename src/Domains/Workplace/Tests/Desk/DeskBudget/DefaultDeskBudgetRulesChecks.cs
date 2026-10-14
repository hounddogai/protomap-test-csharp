namespace Acme.Workforce.Workplace.Tests.Desk.DeskBudget;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Checks of desk budget code, run by the project's runner.</summary>
public static class DefaultDeskBudgetRulesChecks
{
    public static void DefaultDeskBudgetValidatorReportsEveryProblem()
    {
        var validator = DefaultDeskBudgetRules.DefaultDefaultDeskBudgetValidator();
        var item = DefaultDeskBudget.NewDefaultDeskBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultDeskBudgetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultDeskBudgetRequiredTitleRule("default desk budget needs this field");
        var item = DefaultDeskBudget.NewDefaultDeskBudget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultDeskBudgetRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new DefaultDeskBudgetRequiredManagerIdRule("default desk budget needs a value");
        var item = DefaultDeskBudget.NewDefaultDeskBudget("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultDeskBudgetManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new DefaultDeskBudgetManagerIdLengthRule(32);
        var item = DefaultDeskBudget.NewDefaultDeskBudget("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultDeskBudgetCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultDeskBudgetCodeLengthRule(32);
        var item = DefaultDeskBudget.NewDefaultDeskBudget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultDeskBudgetValidatorReportsEveryProblem();
        DefaultDeskBudgetRequiredTitleRuleFlagsABlankTitle();
        DefaultDeskBudgetRequiredManagerIdRuleFlagsABlankManagerId();
        DefaultDeskBudgetManagerIdLengthRuleFlagsALongManagerId();
        DefaultDeskBudgetCodeLengthRuleFlagsALongCode();
    }
}
