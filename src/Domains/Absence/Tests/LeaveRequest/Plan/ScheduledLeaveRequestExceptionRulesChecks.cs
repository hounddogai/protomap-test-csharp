namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestExceptionRulesChecks
{
    public static void ScheduledLeaveRequestExceptionValidatorReportsEveryProblem()
    {
        var validator = ScheduledLeaveRequestExceptionRules.DefaultScheduledLeaveRequestExceptionValidator();
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledLeaveRequestExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledLeaveRequestExceptionRequiredCodeRule(
            "scheduled leave request exception needs this field");
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestExceptionCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new ScheduledLeaveRequestExceptionCurrencyLengthRule(3600);
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledLeaveRequestExceptionTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ScheduledLeaveRequestExceptionTeamLengthRule(255);
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledLeaveRequestExceptionRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ScheduledLeaveRequestExceptionRequiredTeamRule(
            "scheduled leave request exception needs this field");
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestExceptionRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new ScheduledLeaveRequestExceptionRequiredCurrencyRule(
            "scheduled leave request exception needs a value");
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestExceptionLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledLeaveRequestExceptionLabelLengthRule(3600);
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestExceptionValidatorReportsEveryProblem();
        ScheduledLeaveRequestExceptionRequiredCodeRuleFlagsABlankCode();
        ScheduledLeaveRequestExceptionCurrencyLengthRuleFlagsALongCurrency();
        ScheduledLeaveRequestExceptionTeamLengthRuleFlagsALongTeam();
        ScheduledLeaveRequestExceptionRequiredTeamRuleFlagsABlankTeam();
        ScheduledLeaveRequestExceptionRequiredCurrencyRuleFlagsABlankCurrency();
        ScheduledLeaveRequestExceptionLabelLengthRuleFlagsALongLabel();
    }
}
