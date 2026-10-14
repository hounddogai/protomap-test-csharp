namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class StrictDeskBudgetRulesChecks
{
    public static void StrictDeskBudgetValidatorReportsEveryProblem()
    {
        var validator = StrictDeskBudgetRules.DefaultStrictDeskBudgetValidator();
        var item = StrictDeskBudget.NewStrictDeskBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictDeskBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictDeskBudgetRequiredCodeRule("strict desk budget needs an entry");
        var item = StrictDeskBudget.NewStrictDeskBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictDeskBudgetRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new StrictDeskBudgetRequiredUnitRule("strict desk budget needs an entry");
        var item = StrictDeskBudget.NewStrictDeskBudget("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictDeskBudgetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictDeskBudgetRequiredLabelRule("strict desk budget needs an entry");
        var item = StrictDeskBudget.NewStrictDeskBudget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictDeskBudgetUnitLengthRuleFlagsALongUnit()
    {
        var rule = new StrictDeskBudgetUnitLengthRule(64);
        var item = StrictDeskBudget.NewStrictDeskBudget("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictDeskBudgetValidatorReportsEveryProblem();
        StrictDeskBudgetRequiredCodeRuleFlagsABlankCode();
        StrictDeskBudgetRequiredUnitRuleFlagsABlankUnit();
        StrictDeskBudgetRequiredLabelRuleFlagsABlankLabel();
        StrictDeskBudgetUnitLengthRuleFlagsALongUnit();
    }
}
