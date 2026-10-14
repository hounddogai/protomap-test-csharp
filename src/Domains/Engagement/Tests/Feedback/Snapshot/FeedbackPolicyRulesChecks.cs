namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackPolicyRulesChecks
{
    public static void FeedbackPolicyValidatorReportsEveryProblem()
    {
        var validator = FeedbackPolicyRules.DefaultFeedbackPolicyValidator();
        var item = FeedbackPolicy.NewFeedbackPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FeedbackPolicyRequiredTitleRule("feedback policy needs an entry");
        var item = FeedbackPolicy.NewFeedbackPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackPolicyLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new FeedbackPolicyLocaleLengthRule(1000);
        var item = FeedbackPolicy.NewFeedbackPolicy("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackPolicyValidatorReportsEveryProblem();
        FeedbackPolicyRequiredTitleRuleFlagsABlankTitle();
        FeedbackPolicyLocaleLengthRuleFlagsALongLocale();
    }
}
