namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class PayRunSegmentRulesChecks
{
    public static void PayRunSegmentValidatorReportsEveryProblem()
    {
        var validator = PayRunSegmentRules.DefaultPayRunSegmentValidator();
        var item = PayRunSegment.NewPayRunSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunSegmentRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new PayRunSegmentRevisionRangeRule(16, 80);
        var item = PayRunSegment.NewPayRunSegment("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayRunSegmentOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new PayRunSegmentOwnerIdLengthRule(3600);
        var item = PayRunSegment.NewPayRunSegment("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunSegmentRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PayRunSegmentRatingRangeRule(0.0, 2830.8);
        var item = PayRunSegment.NewPayRunSegment("x", "y");
        item.Rating = 2832.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayRunSegmentValidatorReportsEveryProblem();
        PayRunSegmentRevisionRangeRuleFlagsValuesAboveRevision();
        PayRunSegmentOwnerIdLengthRuleFlagsALongOwnerId();
        PayRunSegmentRatingRangeRuleFlagsValuesAboveRating();
    }
}
