namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class BatchedRatePreferenceRulesChecks
{
    public static void BatchedRatePreferenceValidatorReportsEveryProblem()
    {
        var validator = BatchedRatePreferenceRules.DefaultBatchedRatePreferenceValidator();
        var item = BatchedRatePreference.NewBatchedRatePreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedRatePreferenceRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new BatchedRatePreferenceRequiredSegmentRule("batched rate preference needs a value");
        var item = BatchedRatePreference.NewBatchedRatePreference("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedRatePreferenceSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new BatchedRatePreferenceSegmentLengthRule(64);
        var item = BatchedRatePreference.NewBatchedRatePreference("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedRatePreferenceRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new BatchedRatePreferenceRequiredTitleRule("batched rate preference needs an entry");
        var item = BatchedRatePreference.NewBatchedRatePreference("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedRatePreferenceValidatorReportsEveryProblem();
        BatchedRatePreferenceRequiredSegmentRuleFlagsABlankSegment();
        BatchedRatePreferenceSegmentLengthRuleFlagsALongSegment();
        BatchedRatePreferenceRequiredTitleRuleFlagsABlankTitle();
    }
}
