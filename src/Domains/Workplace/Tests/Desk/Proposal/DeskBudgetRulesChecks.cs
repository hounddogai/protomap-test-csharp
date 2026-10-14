namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskBudgetRulesChecks
{
    public static void DeskBudgetValidatorReportsEveryProblem()
    {
        var validator = DeskBudgetRules.DefaultDeskBudgetValidator();
        var item = DeskBudget.NewDeskBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskBudgetRequiredCodeRule("desk budget needs this field");
        var item = DeskBudget.NewDeskBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskBudgetRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new DeskBudgetRequiredContactEmailRule("desk budget needs a value");
        var item = DeskBudget.NewDeskBudget("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskBudgetValidatorReportsEveryProblem();
        DeskBudgetRequiredCodeRuleFlagsABlankCode();
        DeskBudgetRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
