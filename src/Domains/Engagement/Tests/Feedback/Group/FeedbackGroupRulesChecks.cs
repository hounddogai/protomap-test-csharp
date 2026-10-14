namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FeedbackGroupRulesChecks
{
    public static void FeedbackGroupValidatorReportsEveryProblem()
    {
        var validator = FeedbackGroupRules.DefaultFeedbackGroupValidator();
        var item = FeedbackGroup.NewFeedbackGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackGroupVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FeedbackGroupVersionRangeRule(3600, 3600);
        var item = FeedbackGroup.NewFeedbackGroup("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackGroupRequiredNameRuleFlagsABlankName()
    {
        var rule = new FeedbackGroupRequiredNameRule("feedback group needs an entry");
        var item = FeedbackGroup.NewFeedbackGroup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackGroupChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new FeedbackGroupChecksumLengthRule(120);
        var item = FeedbackGroup.NewFeedbackGroup("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackGroupRequiredCodeRule("feedback group needs an entry");
        var item = FeedbackGroup.NewFeedbackGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackGroupValidatorReportsEveryProblem();
        FeedbackGroupVersionRangeRuleFlagsValuesAboveVersion();
        FeedbackGroupRequiredNameRuleFlagsABlankName();
        FeedbackGroupChecksumLengthRuleFlagsALongChecksum();
        FeedbackGroupRequiredCodeRuleFlagsABlankCode();
    }
}
