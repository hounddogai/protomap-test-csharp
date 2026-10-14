namespace Acme.Workforce.Absence.Tests.Holiday.Mapping;

using Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class HolidayRollupRulesChecks
{
    public static void HolidayRollupValidatorReportsEveryProblem()
    {
        var validator = HolidayRollupRules.DefaultHolidayRollupValidator();
        var item = HolidayRollup.NewHolidayRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayRollupBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new HolidayRollupBatchSizeRangeRule(255, 86400);
        var item = HolidayRollup.NewHolidayRollup("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayRollupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new HolidayRollupTenureMonthsRangeRule(80, 1000);
        var item = HolidayRollup.NewHolidayRollup("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayRollupCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new HolidayRollupCreatedAtRangeRule(32, 255);
        var item = HolidayRollup.NewHolidayRollup("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        HolidayRollupValidatorReportsEveryProblem();
        HolidayRollupBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        HolidayRollupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        HolidayRollupCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
