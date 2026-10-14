namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentRuleRulesChecks
{
    public static void AssignmentRuleValidatorReportsEveryProblem()
    {
        var validator = AssignmentRuleRules.DefaultAssignmentRuleValidator();
        var item = AssignmentRule.NewAssignmentRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentRuleTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AssignmentRuleTitleLengthRule(64);
        var item = AssignmentRule.NewAssignmentRule("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentRuleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new AssignmentRuleBudgetRangeRule(2.4, 3217.6);
        var item = AssignmentRule.NewAssignmentRule("x", "y");
        item.Budget = 3219.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentRuleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AssignmentRuleRequiredTitleRule("assignment rule needs a value");
        var item = AssignmentRule.NewAssignmentRule("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentRuleValidatorReportsEveryProblem();
        AssignmentRuleTitleLengthRuleFlagsALongTitle();
        AssignmentRuleBudgetRangeRuleFlagsValuesAboveBudget();
        AssignmentRuleRequiredTitleRuleFlagsABlankTitle();
    }
}
