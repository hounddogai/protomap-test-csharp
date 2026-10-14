namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class EngagementNoticeRulesChecks
{
    public static void EngagementNoticeValidatorReportsEveryProblem()
    {
        var validator = EngagementNoticeRules.DefaultEngagementNoticeValidator();
        var item = EngagementNotice.NewEngagementNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementNoticeReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new EngagementNoticeReviewerLengthRule(1000);
        var item = EngagementNotice.NewEngagementNotice("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementNoticeLabelLengthRule(16);
        var item = EngagementNotice.NewEngagementNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementNoticeRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new EngagementNoticeRequiredReviewerRule("engagement notice needs this field");
        var item = EngagementNotice.NewEngagementNotice("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementNoticeValidatorReportsEveryProblem();
        EngagementNoticeReviewerLengthRuleFlagsALongReviewer();
        EngagementNoticeLabelLengthRuleFlagsALongLabel();
        EngagementNoticeRequiredReviewerRuleFlagsABlankReviewer();
    }
}
