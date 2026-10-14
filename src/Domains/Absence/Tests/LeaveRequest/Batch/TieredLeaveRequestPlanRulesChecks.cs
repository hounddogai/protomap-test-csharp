namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredLeaveRequestPlanRulesChecks
{
    public static void TieredLeaveRequestPlanValidatorReportsEveryProblem()
    {
        var validator = TieredLeaveRequestPlanRules.DefaultTieredLeaveRequestPlanValidator();
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLeaveRequestPlanRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredLeaveRequestPlanRequiredLabelRule("tiered leave request plan needs a value");
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLeaveRequestPlanReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new TieredLeaveRequestPlanReviewerLengthRule(1000);
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredLeaveRequestPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredLeaveRequestPlanCodeLengthRule(86400);
        var item = TieredLeaveRequestPlan.NewTieredLeaveRequestPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLeaveRequestPlanValidatorReportsEveryProblem();
        TieredLeaveRequestPlanRequiredLabelRuleFlagsABlankLabel();
        TieredLeaveRequestPlanReviewerLengthRuleFlagsALongReviewer();
        TieredLeaveRequestPlanCodeLengthRuleFlagsALongCode();
    }
}
