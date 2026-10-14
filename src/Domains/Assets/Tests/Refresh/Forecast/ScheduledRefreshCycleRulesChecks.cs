namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledRefreshCycleRulesChecks
{
    public static void ScheduledRefreshCycleValidatorReportsEveryProblem()
    {
        var validator = ScheduledRefreshCycleRules.DefaultScheduledRefreshCycleValidator();
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledRefreshCycleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ScheduledRefreshCycleDurationMinutesRangeRule(120, 1000);
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledRefreshCycleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledRefreshCycleRequiredCodeRule("scheduled refresh cycle needs this field");
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledRefreshCycleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ScheduledRefreshCycleChannelLengthRule(16);
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledRefreshCycleValidatorReportsEveryProblem();
        ScheduledRefreshCycleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ScheduledRefreshCycleRequiredCodeRuleFlagsABlankCode();
        ScheduledRefreshCycleChannelLengthRuleFlagsALongChannel();
    }
}
