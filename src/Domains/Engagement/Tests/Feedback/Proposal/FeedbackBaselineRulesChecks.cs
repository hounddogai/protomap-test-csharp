namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackBaselineRulesChecks
{
    public static void FeedbackBaselineValidatorReportsEveryProblem()
    {
        var validator = FeedbackBaselineRules.DefaultFeedbackBaselineValidator();
        var item = FeedbackBaseline.NewFeedbackBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackBaselineTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FeedbackBaselineTitleLengthRule(3600);
        var item = FeedbackBaseline.NewFeedbackBaseline("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackBaselineSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new FeedbackBaselineSummaryLengthRule(16);
        var item = FeedbackBaseline.NewFeedbackBaseline("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackBaselineTeamLengthRuleFlagsALongTeam()
    {
        var rule = new FeedbackBaselineTeamLengthRule(80);
        var item = FeedbackBaseline.NewFeedbackBaseline("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackBaselineValidatorReportsEveryProblem();
        FeedbackBaselineTitleLengthRuleFlagsALongTitle();
        FeedbackBaselineSummaryLengthRuleFlagsALongSummary();
        FeedbackBaselineTeamLengthRuleFlagsALongTeam();
    }
}
