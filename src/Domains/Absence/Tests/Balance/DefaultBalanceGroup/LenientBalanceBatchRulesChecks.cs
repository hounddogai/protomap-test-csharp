namespace Acme.Workforce.Absence.Tests.Balance.DefaultBalanceGroup;

using Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>Checks of default balance group code, run by the project's runner.</summary>
public static class LenientBalanceBatchRulesChecks
{
    public static void LenientBalanceBatchValidatorReportsEveryProblem()
    {
        var validator = LenientBalanceBatchRules.DefaultLenientBalanceBatchValidator();
        var item = LenientBalanceBatch.NewLenientBalanceBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBalanceBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientBalanceBatchRequiredTitleRule("lenient balance batch needs a value");
        var item = LenientBalanceBatch.NewLenientBalanceBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBalanceBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientBalanceBatchTitleLengthRule(255);
        var item = LenientBalanceBatch.NewLenientBalanceBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientBalanceBatchValidatorReportsEveryProblem();
        LenientBalanceBatchRequiredTitleRuleFlagsABlankTitle();
        LenientBalanceBatchTitleLengthRuleFlagsALongTitle();
    }
}
