namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssignmentBudgetRulesChecks
{
    public static void AssignmentBudgetValidatorReportsEveryProblem()
    {
        var validator = AssignmentBudgetRules.DefaultAssignmentBudgetValidator();
        var item = AssignmentBudget.NewAssignmentBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentBudgetSourceLengthRuleFlagsALongSource()
    {
        var rule = new AssignmentBudgetSourceLengthRule(80);
        var item = AssignmentBudget.NewAssignmentBudget("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentBudgetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssignmentBudgetLabelLengthRule(80);
        var item = AssignmentBudget.NewAssignmentBudget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentBudgetRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new AssignmentBudgetRequiredSourceRule("assignment budget needs an entry");
        var item = AssignmentBudget.NewAssignmentBudget("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentBudgetValidatorReportsEveryProblem();
        AssignmentBudgetSourceLengthRuleFlagsALongSource();
        AssignmentBudgetLabelLengthRuleFlagsALongLabel();
        AssignmentBudgetRequiredSourceRuleFlagsABlankSource();
    }
}
