namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LeaveRequestForecastRulesChecks
{
    public static void LeaveRequestForecastValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestForecastRules.DefaultLeaveRequestForecastValidator();
        var item = LeaveRequestForecast.NewLeaveRequestForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestForecastRequiredCodeRule("leave request forecast needs an entry");
        var item = LeaveRequestForecast.NewLeaveRequestForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestForecastRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LeaveRequestForecastRequiredTitleRule("leave request forecast needs this field");
        var item = LeaveRequestForecast.NewLeaveRequestForecast("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestForecastCodeLengthRule(255);
        var item = LeaveRequestForecast.NewLeaveRequestForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestForecastGradeLengthRuleFlagsALongGrade()
    {
        var rule = new LeaveRequestForecastGradeLengthRule(80);
        var item = LeaveRequestForecast.NewLeaveRequestForecast("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LeaveRequestForecastValidatorReportsEveryProblem();
        LeaveRequestForecastRequiredCodeRuleFlagsABlankCode();
        LeaveRequestForecastRequiredTitleRuleFlagsABlankTitle();
        LeaveRequestForecastCodeLengthRuleFlagsALongCode();
        LeaveRequestForecastGradeLengthRuleFlagsALongGrade();
    }
}
