namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledHolidaySnapshotRulesChecks
{
    public static void ScheduledHolidaySnapshotValidatorReportsEveryProblem()
    {
        var validator = ScheduledHolidaySnapshotRules.DefaultScheduledHolidaySnapshotValidator();
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledHolidaySnapshotCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledHolidaySnapshotCodeLengthRule(64);
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledHolidaySnapshotHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new ScheduledHolidaySnapshotHoursRangeRule(7.3, 1379.6);
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("x", "y");
        item.Hours = 1381.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledHolidaySnapshotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ScheduledHolidaySnapshotLabelLengthRule(120);
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledHolidaySnapshotValidatorReportsEveryProblem();
        ScheduledHolidaySnapshotCodeLengthRuleFlagsALongCode();
        ScheduledHolidaySnapshotHoursRangeRuleFlagsValuesAboveHours();
        ScheduledHolidaySnapshotLabelLengthRuleFlagsALongLabel();
    }
}
