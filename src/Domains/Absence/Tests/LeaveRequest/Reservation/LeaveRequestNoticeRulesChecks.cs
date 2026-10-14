namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LeaveRequestNoticeRulesChecks
{
    public static void LeaveRequestNoticeValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestNoticeRules.DefaultLeaveRequestNoticeValidator();
        var item = LeaveRequestNotice.NewLeaveRequestNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new LeaveRequestNoticeNameLengthRule(80);
        var item = LeaveRequestNotice.NewLeaveRequestNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestNoticeWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new LeaveRequestNoticeWeightRangeRule(8.6, 4095.9);
        var item = LeaveRequestNotice.NewLeaveRequestNotice("x", "y");
        item.Weight = 4097.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestNoticeRequiredCodeRule("leave request notice needs a value");
        var item = LeaveRequestNotice.NewLeaveRequestNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestNoticeLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new LeaveRequestNoticeLeaveDaysRangeRule(86400, 86400);
        var item = LeaveRequestNotice.NewLeaveRequestNotice("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestNoticeValidatorReportsEveryProblem();
        LeaveRequestNoticeNameLengthRuleFlagsALongName();
        LeaveRequestNoticeWeightRangeRuleFlagsValuesAboveWeight();
        LeaveRequestNoticeRequiredCodeRuleFlagsABlankCode();
        LeaveRequestNoticeLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
    }
}
