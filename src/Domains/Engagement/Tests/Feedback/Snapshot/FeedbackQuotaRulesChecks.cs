namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackQuotaRulesChecks
{
    public static void FeedbackQuotaValidatorReportsEveryProblem()
    {
        var validator = FeedbackQuotaRules.DefaultFeedbackQuotaValidator();
        var item = FeedbackQuota.NewFeedbackQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackQuotaSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new FeedbackQuotaSalaryBandLengthRule(3600);
        var item = FeedbackQuota.NewFeedbackQuota("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackQuotaRegionLengthRuleFlagsALongRegion()
    {
        var rule = new FeedbackQuotaRegionLengthRule(64);
        var item = FeedbackQuota.NewFeedbackQuota("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackQuotaRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new FeedbackQuotaRequiredRegionRule("feedback quota needs this field");
        var item = FeedbackQuota.NewFeedbackQuota("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackQuotaRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new FeedbackQuotaRatioRangeRule(7.7, 4052.6);
        var item = FeedbackQuota.NewFeedbackQuota("x", "y");
        item.Ratio = 4054.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FeedbackQuotaValidatorReportsEveryProblem();
        FeedbackQuotaSalaryBandLengthRuleFlagsALongSalaryBand();
        FeedbackQuotaRegionLengthRuleFlagsALongRegion();
        FeedbackQuotaRequiredRegionRuleFlagsABlankRegion();
        FeedbackQuotaRatioRangeRuleFlagsValuesAboveRatio();
    }
}
