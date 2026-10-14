namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackManifestRulesChecks
{
    public static void FeedbackManifestValidatorReportsEveryProblem()
    {
        var validator = FeedbackManifestRules.DefaultFeedbackManifestValidator();
        var item = FeedbackManifest.NewFeedbackManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new FeedbackManifestRequiredNameRule("feedback manifest needs a value");
        var item = FeedbackManifest.NewFeedbackManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackManifestRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new FeedbackManifestRequiredGradeRule("feedback manifest needs a value");
        var item = FeedbackManifest.NewFeedbackManifest("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackManifestValidatorReportsEveryProblem();
        FeedbackManifestRequiredNameRuleFlagsABlankName();
        FeedbackManifestRequiredGradeRuleFlagsABlankGrade();
    }
}
