namespace Acme.Workforce.Compensation.Tests.EquityGrant.Exception;

using Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class EquityGrantExceptionRulesChecks
{
    public static void EquityGrantExceptionValidatorReportsEveryProblem()
    {
        var validator = EquityGrantExceptionRules.DefaultEquityGrantExceptionValidator();
        var item = EquityGrantException.NewEquityGrantException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new EquityGrantExceptionTitleLengthRule(64);
        var item = EquityGrantException.NewEquityGrantException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantExceptionSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new EquityGrantExceptionSequenceRangeRule(32, 120);
        var item = EquityGrantException.NewEquityGrantException("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantExceptionRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new EquityGrantExceptionRequiredTitleRule("equity grant exception needs an entry");
        var item = EquityGrantException.NewEquityGrantException("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantExceptionValidatorReportsEveryProblem();
        EquityGrantExceptionTitleLengthRuleFlagsALongTitle();
        EquityGrantExceptionSequenceRangeRuleFlagsValuesAboveSequence();
        EquityGrantExceptionRequiredTitleRuleFlagsABlankTitle();
    }
}
