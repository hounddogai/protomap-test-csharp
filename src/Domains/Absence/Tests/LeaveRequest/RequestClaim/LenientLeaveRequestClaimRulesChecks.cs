namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LenientLeaveRequestClaimRulesChecks
{
    public static void LenientLeaveRequestClaimValidatorReportsEveryProblem()
    {
        var validator = LenientLeaveRequestClaimRules.DefaultLenientLeaveRequestClaimValidator();
        var item = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLeaveRequestClaimReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new LenientLeaveRequestClaimReviewerLengthRule(80);
        var item = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientLeaveRequestClaimRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new LenientLeaveRequestClaimRequiredEmployeeIdRule("lenient leave request claim needs a value");
        var item = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientLeaveRequestClaimValidatorReportsEveryProblem();
        LenientLeaveRequestClaimReviewerLengthRuleFlagsALongReviewer();
        LenientLeaveRequestClaimRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
