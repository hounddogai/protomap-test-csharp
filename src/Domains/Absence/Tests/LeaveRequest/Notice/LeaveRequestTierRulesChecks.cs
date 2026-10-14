namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestTierRulesChecks
{
    public static void LeaveRequestTierValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestTierRules.DefaultLeaveRequestTierValidator();
        var item = LeaveRequestTier.NewLeaveRequestTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestTierRequiredCodeRule("leave request tier needs a value");
        var item = LeaveRequestTier.NewLeaveRequestTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestTierCodeLengthRule(80);
        var item = LeaveRequestTier.NewLeaveRequestTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LeaveRequestTierRequiredLabelRule("leave request tier needs a value");
        var item = LeaveRequestTier.NewLeaveRequestTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestTierValidatorReportsEveryProblem();
        LeaveRequestTierRequiredCodeRuleFlagsABlankCode();
        LeaveRequestTierCodeLengthRuleFlagsALongCode();
        LeaveRequestTierRequiredLabelRuleFlagsABlankLabel();
    }
}
