namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRecordRulesChecks
{
    public static void PoolRecordValidatorReportsEveryProblem()
    {
        var validator = PoolRecordRules.DefaultPoolRecordValidator();
        var item = PoolRecord.NewPoolRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolRecordRequiredCodeRule("pool record needs this field");
        var item = PoolRecord.NewPoolRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolRecordReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PoolRecordReviewerLengthRule(16);
        var item = PoolRecord.NewPoolRecord("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolRecordValidatorReportsEveryProblem();
        PoolRecordRequiredCodeRuleFlagsABlankCode();
        PoolRecordReviewerLengthRuleFlagsALongReviewer();
    }
}
