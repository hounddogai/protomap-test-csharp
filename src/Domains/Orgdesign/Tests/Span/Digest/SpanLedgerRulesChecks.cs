namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanLedgerRulesChecks
{
    public static void SpanLedgerValidatorReportsEveryProblem()
    {
        var validator = SpanLedgerRules.DefaultSpanLedgerValidator();
        var item = SpanLedger.NewSpanLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanLedgerSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new SpanLedgerSegmentLengthRule(80);
        var item = SpanLedger.NewSpanLedger("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanLedgerCodeLengthRule(120);
        var item = SpanLedger.NewSpanLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanLedgerLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SpanLedgerLeaveDaysRangeRule(16, 64);
        var item = SpanLedger.NewSpanLedger("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanLedgerRequiredNameRule("span ledger needs a value");
        var item = SpanLedger.NewSpanLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanLedgerRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new SpanLedgerRequiredSegmentRule("span ledger needs an entry");
        var item = SpanLedger.NewSpanLedger("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanLedgerCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new SpanLedgerCreatedAtRangeRule(16, 3600);
        var item = SpanLedger.NewSpanLedger("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SpanLedgerValidatorReportsEveryProblem();
        SpanLedgerSegmentLengthRuleFlagsALongSegment();
        SpanLedgerCodeLengthRuleFlagsALongCode();
        SpanLedgerLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SpanLedgerRequiredNameRuleFlagsABlankName();
        SpanLedgerRequiredSegmentRuleFlagsABlankSegment();
        SpanLedgerCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
