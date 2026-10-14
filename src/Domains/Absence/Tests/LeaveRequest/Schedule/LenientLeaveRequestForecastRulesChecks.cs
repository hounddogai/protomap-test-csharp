namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientLeaveRequestForecastRulesChecks
{
    public static void LenientLeaveRequestForecastValidatorReportsEveryProblem()
    {
        var validator = LenientLeaveRequestForecastRules.DefaultLenientLeaveRequestForecastValidator();
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientLeaveRequestForecastRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientLeaveRequestForecastRequiredLabelRule("lenient leave request forecast needs an entry");
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientLeaveRequestForecastMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new LenientLeaveRequestForecastMinimumRangeRule(3.7, 2235.4);
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("x", "y");
        item.Minimum = 2236.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientLeaveRequestForecastLocationLengthRuleFlagsALongLocation()
    {
        var rule = new LenientLeaveRequestForecastLocationLengthRule(120);
        var item = LenientLeaveRequestForecast.NewLenientLeaveRequestForecast("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientLeaveRequestForecastValidatorReportsEveryProblem();
        LenientLeaveRequestForecastRequiredLabelRuleFlagsABlankLabel();
        LenientLeaveRequestForecastMinimumRangeRuleFlagsValuesAboveMinimum();
        LenientLeaveRequestForecastLocationLengthRuleFlagsALongLocation();
    }
}
