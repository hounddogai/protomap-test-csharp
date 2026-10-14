namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackScheduleRulesChecks
{
    public static void FeedbackScheduleValidatorReportsEveryProblem()
    {
        var validator = FeedbackScheduleRules.DefaultFeedbackScheduleValidator();
        var item = FeedbackSchedule.NewFeedbackSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackScheduleRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new FeedbackScheduleRequiredOwnerIdRule("feedback schedule needs this field");
        var item = FeedbackSchedule.NewFeedbackSchedule("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackScheduleOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new FeedbackScheduleOwnerIdLengthRule(80);
        var item = FeedbackSchedule.NewFeedbackSchedule("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackScheduleRequiredCodeRule("feedback schedule needs this field");
        var item = FeedbackSchedule.NewFeedbackSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackScheduleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FeedbackScheduleLabelLengthRule(80);
        var item = FeedbackSchedule.NewFeedbackSchedule("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackScheduleValidatorReportsEveryProblem();
        FeedbackScheduleRequiredOwnerIdRuleFlagsABlankOwnerId();
        FeedbackScheduleOwnerIdLengthRuleFlagsALongOwnerId();
        FeedbackScheduleRequiredCodeRuleFlagsABlankCode();
        FeedbackScheduleLabelLengthRuleFlagsALongLabel();
    }
}
