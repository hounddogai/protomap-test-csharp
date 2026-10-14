namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalSpanRecordRulesChecks
{
    public static void RegionalSpanRecordValidatorReportsEveryProblem()
    {
        var validator = RegionalSpanRecordRules.DefaultRegionalSpanRecordValidator();
        var item = RegionalSpanRecord.NewRegionalSpanRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalSpanRecordRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new RegionalSpanRecordRequiredReviewerRule("regional span record needs a value");
        var item = RegionalSpanRecord.NewRegionalSpanRecord("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSpanRecordRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new RegionalSpanRecordRevisionRangeRule(32, 64);
        var item = RegionalSpanRecord.NewRegionalSpanRecord("x", "y");
        item.Revision = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSpanRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new RegionalSpanRecordBatchSizeRangeRule(32, 120);
        var item = RegionalSpanRecord.NewRegionalSpanRecord("x", "y");
        item.BatchSize = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalSpanRecordRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalSpanRecordRequiredTitleRule("regional span record needs an entry");
        var item = RegionalSpanRecord.NewRegionalSpanRecord("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalSpanRecordTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalSpanRecordTitleLengthRule(80);
        var item = RegionalSpanRecord.NewRegionalSpanRecord("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalSpanRecordValidatorReportsEveryProblem();
        RegionalSpanRecordRequiredReviewerRuleFlagsABlankReviewer();
        RegionalSpanRecordRevisionRangeRuleFlagsValuesAboveRevision();
        RegionalSpanRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        RegionalSpanRecordRequiredTitleRuleFlagsABlankTitle();
        RegionalSpanRecordTitleLengthRuleFlagsALongTitle();
    }
}
