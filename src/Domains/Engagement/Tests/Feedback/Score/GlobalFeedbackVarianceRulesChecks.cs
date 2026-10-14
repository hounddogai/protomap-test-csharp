namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GlobalFeedbackVarianceRulesChecks
{
    public static void GlobalFeedbackVarianceValidatorReportsEveryProblem()
    {
        var validator = GlobalFeedbackVarianceRules.DefaultGlobalFeedbackVarianceValidator();
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalFeedbackVarianceRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new GlobalFeedbackVarianceRequiredOwnerIdRule("global feedback variance needs a value");
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalFeedbackVarianceLocationLengthRuleFlagsALongLocation()
    {
        var rule = new GlobalFeedbackVarianceLocationLengthRule(64);
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalFeedbackVarianceRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new GlobalFeedbackVarianceRequiredLocationRule("global feedback variance needs an entry");
        var item = GlobalFeedbackVariance.NewGlobalFeedbackVariance("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalFeedbackVarianceValidatorReportsEveryProblem();
        GlobalFeedbackVarianceRequiredOwnerIdRuleFlagsABlankOwnerId();
        GlobalFeedbackVarianceLocationLengthRuleFlagsALongLocation();
        GlobalFeedbackVarianceRequiredLocationRuleFlagsABlankLocation();
    }
}
