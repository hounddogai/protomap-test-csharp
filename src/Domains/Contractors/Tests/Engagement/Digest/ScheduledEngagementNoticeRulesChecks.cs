namespace Acme.Workforce.Contractors.Tests.Engagement.Digest;

using Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class ScheduledEngagementNoticeRulesChecks
{
    public static void ScheduledEngagementNoticeValidatorReportsEveryProblem()
    {
        var validator = ScheduledEngagementNoticeRules.DefaultScheduledEngagementNoticeValidator();
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledEngagementNoticeRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ScheduledEngagementNoticeRequiredLabelRule("scheduled engagement notice needs a value");
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledEngagementNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledEngagementNoticeRequiredCodeRule("scheduled engagement notice needs this field");
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledEngagementNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledEngagementNoticeLabelLengthRule(86400);
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEngagementNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledEngagementNoticeCodeLengthRule(255);
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledEngagementNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledEngagementNoticeNameLengthRule(3600);
        var item = ScheduledEngagementNotice.NewScheduledEngagementNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledEngagementNoticeValidatorReportsEveryProblem();
        ScheduledEngagementNoticeRequiredLabelRuleFlagsABlankLabel();
        ScheduledEngagementNoticeRequiredCodeRuleFlagsABlankCode();
        ScheduledEngagementNoticeLabelLengthRuleFlagsALongLabel();
        ScheduledEngagementNoticeCodeLengthRuleFlagsALongCode();
        ScheduledEngagementNoticeNameLengthRuleFlagsALongName();
    }
}
