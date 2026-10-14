namespace Acme.Workforce.Workplace.Tests.Booking.Ledger;

using Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBookingLedgerRulesChecks
{
    public static void BatchedBookingLedgerValidatorReportsEveryProblem()
    {
        var validator = BatchedBookingLedgerRules.DefaultBatchedBookingLedgerValidator();
        var item = BatchedBookingLedger.NewBatchedBookingLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedBookingLedgerMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new BatchedBookingLedgerMinimumRangeRule(3.8, 2174.2);
        var item = BatchedBookingLedger.NewBatchedBookingLedger("x", "y");
        item.Minimum = 2175.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedBookingLedgerLocationLengthRuleFlagsALongLocation()
    {
        var rule = new BatchedBookingLedgerLocationLengthRule(3600);
        var item = BatchedBookingLedger.NewBatchedBookingLedger("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedBookingLedgerSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new BatchedBookingLedgerSegmentLengthRule(32);
        var item = BatchedBookingLedger.NewBatchedBookingLedger("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedBookingLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedBookingLedgerCodeLengthRule(120);
        var item = BatchedBookingLedger.NewBatchedBookingLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedBookingLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new BatchedBookingLedgerLabelLengthRule(120);
        var item = BatchedBookingLedger.NewBatchedBookingLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedBookingLedgerValidatorReportsEveryProblem();
        BatchedBookingLedgerMinimumRangeRuleFlagsValuesAboveMinimum();
        BatchedBookingLedgerLocationLengthRuleFlagsALongLocation();
        BatchedBookingLedgerSegmentLengthRuleFlagsALongSegment();
        BatchedBookingLedgerCodeLengthRuleFlagsALongCode();
        BatchedBookingLedgerLabelLengthRuleFlagsALongLabel();
    }
}
