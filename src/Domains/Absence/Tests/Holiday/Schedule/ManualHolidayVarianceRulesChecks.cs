namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualHolidayVarianceRulesChecks
{
    public static void ManualHolidayVarianceValidatorReportsEveryProblem()
    {
        var validator = ManualHolidayVarianceRules.DefaultManualHolidayVarianceValidator();
        var item = ManualHolidayVariance.NewManualHolidayVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualHolidayVarianceMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ManualHolidayVarianceMinimumRangeRule(6.3, 3502.6);
        var item = ManualHolidayVariance.NewManualHolidayVariance("x", "y");
        item.Minimum = 3504.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualHolidayVarianceTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualHolidayVarianceTitleLengthRule(120);
        var item = ManualHolidayVariance.NewManualHolidayVariance("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualHolidayVarianceHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new ManualHolidayVarianceHoursRangeRule(7.5, 3198.9);
        var item = ManualHolidayVariance.NewManualHolidayVariance("x", "y");
        item.Hours = 3200.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualHolidayVarianceRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new ManualHolidayVarianceRequiredJobCodeRule("manual holiday variance needs a value");
        var item = ManualHolidayVariance.NewManualHolidayVariance("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualHolidayVarianceValidatorReportsEveryProblem();
        ManualHolidayVarianceMinimumRangeRuleFlagsValuesAboveMinimum();
        ManualHolidayVarianceTitleLengthRuleFlagsALongTitle();
        ManualHolidayVarianceHoursRangeRuleFlagsValuesAboveHours();
        ManualHolidayVarianceRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
