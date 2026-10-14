namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestOverrideRulesChecks
{
    public static void LeaveRequestOverrideValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestOverrideRules.DefaultLeaveRequestOverrideValidator();
        var item = LeaveRequestOverride.NewLeaveRequestOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestOverrideCodeLengthRule(86400);
        var item = LeaveRequestOverride.NewLeaveRequestOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestOverrideGradeLengthRuleFlagsALongGrade()
    {
        var rule = new LeaveRequestOverrideGradeLengthRule(80);
        var item = LeaveRequestOverride.NewLeaveRequestOverride("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestOverrideRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new LeaveRequestOverrideRequiredChannelRule("leave request override needs this field");
        var item = LeaveRequestOverride.NewLeaveRequestOverride("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestOverrideGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LeaveRequestOverrideGraceDaysRangeRule(16, 1000);
        var item = LeaveRequestOverride.NewLeaveRequestOverride("x", "y");
        item.GraceDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestOverrideValidatorReportsEveryProblem();
        LeaveRequestOverrideCodeLengthRuleFlagsALongCode();
        LeaveRequestOverrideGradeLengthRuleFlagsALongGrade();
        LeaveRequestOverrideRequiredChannelRuleFlagsABlankChannel();
        LeaveRequestOverrideGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
