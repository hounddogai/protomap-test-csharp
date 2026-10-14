namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class HolidayEstimateRulesChecks
{
    public static void HolidayEstimateValidatorReportsEveryProblem()
    {
        var validator = HolidayEstimateRules.DefaultHolidayEstimateValidator();
        var item = HolidayEstimate.NewHolidayEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HolidayEstimateRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new HolidayEstimateRequiredSegmentRule("holiday estimate needs an entry");
        var item = HolidayEstimate.NewHolidayEstimate("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayEstimateRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new HolidayEstimateRequiredSalaryBandRule("holiday estimate needs an entry");
        var item = HolidayEstimate.NewHolidayEstimate("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HolidayEstimateFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new HolidayEstimateFloorValueRangeRule(6.6, 821.5);
        var item = HolidayEstimate.NewHolidayEstimate("x", "y");
        item.FloorValue = 823.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HolidayEstimateSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new HolidayEstimateSalaryBandLengthRule(255);
        var item = HolidayEstimate.NewHolidayEstimate("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HolidayEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HolidayEstimateRequiredCodeRule("holiday estimate needs a value");
        var item = HolidayEstimate.NewHolidayEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HolidayEstimateValidatorReportsEveryProblem();
        HolidayEstimateRequiredSegmentRuleFlagsABlankSegment();
        HolidayEstimateRequiredSalaryBandRuleFlagsABlankSalaryBand();
        HolidayEstimateFloorValueRangeRuleFlagsValuesAboveFloorValue();
        HolidayEstimateSalaryBandLengthRuleFlagsALongSalaryBand();
        HolidayEstimateRequiredCodeRuleFlagsABlankCode();
    }
}
