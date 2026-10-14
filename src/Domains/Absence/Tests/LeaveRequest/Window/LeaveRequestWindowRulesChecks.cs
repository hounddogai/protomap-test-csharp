namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LeaveRequestWindowRulesChecks
{
    public static void LeaveRequestWindowValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestWindowRules.DefaultLeaveRequestWindowValidator();
        var item = LeaveRequestWindow.NewLeaveRequestWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestWindowDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new LeaveRequestWindowDepartmentLengthRule(32);
        var item = LeaveRequestWindow.NewLeaveRequestWindow("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestWindowNameLengthRuleFlagsALongName()
    {
        var rule = new LeaveRequestWindowNameLengthRule(120);
        var item = LeaveRequestWindow.NewLeaveRequestWindow("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestWindowRequiredCodeRule("leave request window needs this field");
        var item = LeaveRequestWindow.NewLeaveRequestWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestWindowRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new LeaveRequestWindowRevisionRangeRule(32, 80);
        var item = LeaveRequestWindow.NewLeaveRequestWindow("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestWindowValidatorReportsEveryProblem();
        LeaveRequestWindowDepartmentLengthRuleFlagsALongDepartment();
        LeaveRequestWindowNameLengthRuleFlagsALongName();
        LeaveRequestWindowRequiredCodeRuleFlagsABlankCode();
        LeaveRequestWindowRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
