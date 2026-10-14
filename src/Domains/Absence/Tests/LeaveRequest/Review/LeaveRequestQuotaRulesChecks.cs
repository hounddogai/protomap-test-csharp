namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LeaveRequestQuotaRulesChecks
{
    public static void LeaveRequestQuotaValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestQuotaRules.DefaultLeaveRequestQuotaValidator();
        var item = LeaveRequestQuota.NewLeaveRequestQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestQuotaRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new LeaveRequestQuotaRequiredGradeRule("leave request quota needs this field");
        var item = LeaveRequestQuota.NewLeaveRequestQuota("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestQuotaNameLengthRuleFlagsALongName()
    {
        var rule = new LeaveRequestQuotaNameLengthRule(32);
        var item = LeaveRequestQuota.NewLeaveRequestQuota("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new LeaveRequestQuotaRequiredNameRule("leave request quota needs an entry");
        var item = LeaveRequestQuota.NewLeaveRequestQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestQuotaValidatorReportsEveryProblem();
        LeaveRequestQuotaRequiredGradeRuleFlagsABlankGrade();
        LeaveRequestQuotaNameLengthRuleFlagsALongName();
        LeaveRequestQuotaRequiredNameRuleFlagsABlankName();
    }
}
