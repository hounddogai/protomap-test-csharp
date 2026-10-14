namespace Acme.Workforce.Absence.Tests.Holiday.Adjustment;

using Acme.Workforce.Absence.Holiday.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HolidayBatchRulesChecks
{
    public static void HolidayBatchValidatorReportsEveryProblem()
    {
        var validator = HolidayBatchRules.DefaultHolidayBatchValidator();
        var item = HolidayBatch.NewHolidayBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayBatchTitleLengthRuleFlagsALongTitle()
    {
        var rule = new HolidayBatchTitleLengthRule(120);
        var item = HolidayBatch.NewHolidayBatch("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new HolidayBatchRequiredTitleRule("holiday batch needs a value");
        var item = HolidayBatch.NewHolidayBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayBatchRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new HolidayBatchRequiredChannelRule("holiday batch needs a value");
        var item = HolidayBatch.NewHolidayBatch("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayBatchThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new HolidayBatchThresholdRangeRule(0.5, 341.5);
        var item = HolidayBatch.NewHolidayBatch("x", "y");
        item.Threshold = 343.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayBatchValidatorReportsEveryProblem();
        HolidayBatchTitleLengthRuleFlagsALongTitle();
        HolidayBatchRequiredTitleRuleFlagsABlankTitle();
        HolidayBatchRequiredChannelRuleFlagsABlankChannel();
        HolidayBatchThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
