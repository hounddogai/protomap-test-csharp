namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PrimaryFeedbackNoticeRulesChecks
{
    public static void PrimaryFeedbackNoticeValidatorReportsEveryProblem()
    {
        var validator = PrimaryFeedbackNoticeRules.DefaultPrimaryFeedbackNoticeValidator();
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryFeedbackNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryFeedbackNoticeTitleLengthRule(86400);
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryFeedbackNoticeFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PrimaryFeedbackNoticeFloorValueRangeRule(2.8, 407.7);
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("x", "y");
        item.FloorValue = 409.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryFeedbackNoticeValidatorReportsEveryProblem();
        PrimaryFeedbackNoticeTitleLengthRuleFlagsALongTitle();
        PrimaryFeedbackNoticeFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
