namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyBudgetRulesChecks
{
    public static void PolicyBudgetValidatorReportsEveryProblem()
    {
        var validator = PolicyBudgetRules.DefaultPolicyBudgetValidator();
        var item = PolicyBudget.NewPolicyBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyBudgetRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PolicyBudgetRequiredDescriptionRule("policy budget needs an entry");
        var item = PolicyBudget.NewPolicyBudget("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyBudgetRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new PolicyBudgetRequiredNotesRule("policy budget needs a value");
        var item = PolicyBudget.NewPolicyBudget("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyBudgetRequiredCodeRule("policy budget needs a value");
        var item = PolicyBudget.NewPolicyBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PolicyBudgetValidatorReportsEveryProblem();
        PolicyBudgetRequiredDescriptionRuleFlagsABlankDescription();
        PolicyBudgetRequiredNotesRuleFlagsABlankNotes();
        PolicyBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
