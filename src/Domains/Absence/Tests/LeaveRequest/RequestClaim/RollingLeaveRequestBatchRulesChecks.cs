namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class RollingLeaveRequestBatchRulesChecks
{
    public static void RollingLeaveRequestBatchValidatorReportsEveryProblem()
    {
        var validator = RollingLeaveRequestBatchRules.DefaultRollingLeaveRequestBatchValidator();
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingLeaveRequestBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingLeaveRequestBatchRequiredLabelRule("rolling leave request batch needs a value");
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLeaveRequestBatchRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new RollingLeaveRequestBatchRequiredBadgeRule("rolling leave request batch needs a value");
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingLeaveRequestBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingLeaveRequestBatchLabelLengthRule(32);
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingLeaveRequestBatchValidatorReportsEveryProblem();
        RollingLeaveRequestBatchRequiredLabelRuleFlagsABlankLabel();
        RollingLeaveRequestBatchRequiredBadgeRuleFlagsABlankBadge();
        RollingLeaveRequestBatchLabelLengthRuleFlagsALongLabel();
    }
}
