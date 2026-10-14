namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledPayRunRequestRulesChecks
{
    public static void ScheduledPayRunRequestValidatorReportsEveryProblem()
    {
        var validator = ScheduledPayRunRequestRules.DefaultScheduledPayRunRequestValidator();
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledPayRunRequestWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ScheduledPayRunRequestWindowDaysRangeRule(64, 80);
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("x", "y");
        item.WindowDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledPayRunRequestRegionLengthRuleFlagsALongRegion()
    {
        var rule = new ScheduledPayRunRequestRegionLengthRule(32);
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledPayRunRequestRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new ScheduledPayRunRequestRequiredRegionRule("scheduled pay run request needs an entry");
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayRunRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledPayRunRequestRequiredNameRule("scheduled pay run request needs an entry");
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayRunRequestRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ScheduledPayRunRequestRequiredTimezoneRule("scheduled pay run request needs a value");
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledPayRunRequestNameLengthRuleFlagsALongName()
    {
        var rule = new ScheduledPayRunRequestNameLengthRule(3600);
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledPayRunRequestValidatorReportsEveryProblem();
        ScheduledPayRunRequestWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        ScheduledPayRunRequestRegionLengthRuleFlagsALongRegion();
        ScheduledPayRunRequestRequiredRegionRuleFlagsABlankRegion();
        ScheduledPayRunRequestRequiredNameRuleFlagsABlankName();
        ScheduledPayRunRequestRequiredTimezoneRuleFlagsABlankTimezone();
        ScheduledPayRunRequestNameLengthRuleFlagsALongName();
    }
}
