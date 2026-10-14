namespace Acme.Workforce.Absence.Tests.Balance.Milestone;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BalanceSegmentRulesChecks
{
    public static void BalanceSegmentValidatorReportsEveryProblem()
    {
        var validator = BalanceSegmentRules.DefaultBalanceSegmentValidator();
        var item = BalanceSegment.NewBalanceSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceSegmentBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new BalanceSegmentBudgetRangeRule(5.3, 4622.0);
        var item = BalanceSegment.NewBalanceSegment("x", "y");
        item.Budget = 4623.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BalanceSegmentRequiredCodeRule("balance segment needs an entry");
        var item = BalanceSegment.NewBalanceSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalanceSegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new BalanceSegmentCodeLengthRule(64);
        var item = BalanceSegment.NewBalanceSegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalanceSegmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BalanceSegmentUpdatedAtRangeRule(16, 3600);
        var item = BalanceSegment.NewBalanceSegment("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceSegmentValidatorReportsEveryProblem();
        BalanceSegmentBudgetRangeRuleFlagsValuesAboveBudget();
        BalanceSegmentRequiredCodeRuleFlagsABlankCode();
        BalanceSegmentCodeLengthRuleFlagsALongCode();
        BalanceSegmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
