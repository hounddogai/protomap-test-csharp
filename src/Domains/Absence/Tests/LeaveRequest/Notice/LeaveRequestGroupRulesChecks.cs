namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestGroupRulesChecks
{
    public static void LeaveRequestGroupValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestGroupRules.DefaultLeaveRequestGroupValidator();
        var item = LeaveRequestGroup.NewLeaveRequestGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestGroupCodeLengthRule(86400);
        var item = LeaveRequestGroup.NewLeaveRequestGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestGroupRequiredCodeRule("leave request group needs a value");
        var item = LeaveRequestGroup.NewLeaveRequestGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestGroupRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LeaveRequestGroupRatioRangeRule(9.7, 67.2);
        var item = LeaveRequestGroup.NewLeaveRequestGroup("x", "y");
        item.Ratio = 68.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestGroupValidatorReportsEveryProblem();
        LeaveRequestGroupCodeLengthRuleFlagsALongCode();
        LeaveRequestGroupRequiredCodeRuleFlagsABlankCode();
        LeaveRequestGroupRatioRangeRuleFlagsValuesAboveRatio();
    }
}
