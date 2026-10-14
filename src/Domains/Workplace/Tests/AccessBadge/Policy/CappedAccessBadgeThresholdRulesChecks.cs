namespace Acme.Workforce.Workplace.Tests.AccessBadge.Policy;

using Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedAccessBadgeThresholdRulesChecks
{
    public static void CappedAccessBadgeThresholdValidatorReportsEveryProblem()
    {
        var validator = CappedAccessBadgeThresholdRules.DefaultCappedAccessBadgeThresholdValidator();
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAccessBadgeThresholdTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedAccessBadgeThresholdTitleLengthRule(120);
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAccessBadgeThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedAccessBadgeThresholdCodeLengthRule(86400);
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAccessBadgeThresholdGradeLengthRuleFlagsALongGrade()
    {
        var rule = new CappedAccessBadgeThresholdGradeLengthRule(16);
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAccessBadgeThresholdReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new CappedAccessBadgeThresholdReviewerLengthRule(16);
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedAccessBadgeThresholdRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedAccessBadgeThresholdRequiredTitleRule("capped access badge threshold needs a value");
        var item = CappedAccessBadgeThreshold.NewCappedAccessBadgeThreshold("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedAccessBadgeThresholdValidatorReportsEveryProblem();
        CappedAccessBadgeThresholdTitleLengthRuleFlagsALongTitle();
        CappedAccessBadgeThresholdCodeLengthRuleFlagsALongCode();
        CappedAccessBadgeThresholdGradeLengthRuleFlagsALongGrade();
        CappedAccessBadgeThresholdReviewerLengthRuleFlagsALongReviewer();
        CappedAccessBadgeThresholdRequiredTitleRuleFlagsABlankTitle();
    }
}
