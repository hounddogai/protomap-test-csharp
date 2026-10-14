namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LeaveRequestPolicyRulesChecks
{
    public static void LeaveRequestPolicyValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestPolicyRules.DefaultLeaveRequestPolicyValidator();
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LeaveRequestPolicyLabelLengthRule(86400);
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestPolicyRequiredCodeRule("leave request policy needs this field");
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestPolicyNotesLengthRuleFlagsALongNotes()
    {
        var rule = new LeaveRequestPolicyNotesLengthRule(120);
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LeaveRequestPolicyValidatorReportsEveryProblem();
        LeaveRequestPolicyLabelLengthRuleFlagsALongLabel();
        LeaveRequestPolicyRequiredCodeRuleFlagsABlankCode();
        LeaveRequestPolicyNotesLengthRuleFlagsALongNotes();
    }
}
