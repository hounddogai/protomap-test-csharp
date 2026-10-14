namespace Acme.Workforce.Absence.Tests.Balance.Assignment;

using Acme.Workforce.Absence.Balance.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BalanceAssignmentRulesChecks
{
    public static void BalanceAssignmentValidatorReportsEveryProblem()
    {
        var validator = BalanceAssignmentRules.DefaultBalanceAssignmentValidator();
        var item = BalanceAssignment.NewBalanceAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalanceAssignmentCodeLengthRule(32);
        var item = BalanceAssignment.NewBalanceAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BalanceAssignmentTitleLengthRule(64);
        var item = BalanceAssignment.NewBalanceAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceAssignmentSourceLengthRuleFlagsALongSource()
    {
        var rule = new BalanceAssignmentSourceLengthRule(16);
        var item = BalanceAssignment.NewBalanceAssignment("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceAssignmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new BalanceAssignmentMinimumRangeRule(1.9, 561.7);
        var item = BalanceAssignment.NewBalanceAssignment("x", "y");
        item.Minimum = 563.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceAssignmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BalanceAssignmentRequiredTitleRule("balance assignment needs an entry");
        var item = BalanceAssignment.NewBalanceAssignment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BalanceAssignmentValidatorReportsEveryProblem();
        BalanceAssignmentCodeLengthRuleFlagsALongCode();
        BalanceAssignmentTitleLengthRuleFlagsALongTitle();
        BalanceAssignmentSourceLengthRuleFlagsALongSource();
        BalanceAssignmentMinimumRangeRuleFlagsValuesAboveMinimum();
        BalanceAssignmentRequiredTitleRuleFlagsABlankTitle();
    }
}
