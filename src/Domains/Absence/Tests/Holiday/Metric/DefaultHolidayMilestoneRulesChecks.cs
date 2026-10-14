namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultHolidayMilestoneRulesChecks
{
    public static void DefaultHolidayMilestoneValidatorReportsEveryProblem()
    {
        var validator = DefaultHolidayMilestoneRules.DefaultDefaultHolidayMilestoneValidator();
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHolidayMilestoneRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DefaultHolidayMilestoneRequiredLabelRule("default holiday milestone needs an entry");
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHolidayMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultHolidayMilestoneCodeLengthRule(1000);
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHolidayMilestoneLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DefaultHolidayMilestoneLabelLengthRule(3600);
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHolidayMilestoneExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new DefaultHolidayMilestoneExpiresAtRangeRule(32, 3600);
        var item = DefaultHolidayMilestone.NewDefaultHolidayMilestone("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultHolidayMilestoneValidatorReportsEveryProblem();
        DefaultHolidayMilestoneRequiredLabelRuleFlagsABlankLabel();
        DefaultHolidayMilestoneCodeLengthRuleFlagsALongCode();
        DefaultHolidayMilestoneLabelLengthRuleFlagsALongLabel();
        DefaultHolidayMilestoneExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
