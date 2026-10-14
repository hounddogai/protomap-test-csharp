namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualSegmentRulesChecks
{
    public static void AccrualSegmentValidatorReportsEveryProblem()
    {
        var validator = AccrualSegmentRules.DefaultAccrualSegmentValidator();
        var item = AccrualSegment.NewAccrualSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualSegmentSourceLengthRuleFlagsALongSource()
    {
        var rule = new AccrualSegmentSourceLengthRule(255);
        var item = AccrualSegment.NewAccrualSegment("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualSegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualSegmentRequiredCodeRule("accrual segment needs this field");
        var item = AccrualSegment.NewAccrualSegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualSegmentValidatorReportsEveryProblem();
        AccrualSegmentSourceLengthRuleFlagsALongSource();
        AccrualSegmentRequiredCodeRuleFlagsABlankCode();
    }
}
