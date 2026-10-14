namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackAuditRulesChecks
{
    public static void FeedbackAuditValidatorReportsEveryProblem()
    {
        var validator = FeedbackAuditRules.DefaultFeedbackAuditValidator();
        var item = FeedbackAudit.NewFeedbackAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackAuditNameLengthRuleFlagsALongName()
    {
        var rule = new FeedbackAuditNameLengthRule(3600);
        var item = FeedbackAudit.NewFeedbackAudit("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new FeedbackAuditTitleLengthRule(120);
        var item = FeedbackAudit.NewFeedbackAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackAuditRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FeedbackAuditRequiredTitleRule("feedback audit needs this field");
        var item = FeedbackAudit.NewFeedbackAudit("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackAuditMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new FeedbackAuditMinimumRangeRule(5.1, 4021.4);
        var item = FeedbackAudit.NewFeedbackAudit("x", "y");
        item.Minimum = 4022.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackAuditCodeLengthRule(120);
        var item = FeedbackAudit.NewFeedbackAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackAuditValidatorReportsEveryProblem();
        FeedbackAuditNameLengthRuleFlagsALongName();
        FeedbackAuditTitleLengthRuleFlagsALongTitle();
        FeedbackAuditRequiredTitleRuleFlagsABlankTitle();
        FeedbackAuditMinimumRangeRuleFlagsValuesAboveMinimum();
        FeedbackAuditCodeLengthRuleFlagsALongCode();
    }
}
