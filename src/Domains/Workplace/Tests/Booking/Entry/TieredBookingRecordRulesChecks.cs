namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TieredBookingRecordRulesChecks
{
    public static void TieredBookingRecordValidatorReportsEveryProblem()
    {
        var validator = TieredBookingRecordRules.DefaultTieredBookingRecordValidator();
        var item = TieredBookingRecord.NewTieredBookingRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredBookingRecordRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new TieredBookingRecordRateRangeRule(7.5, 957.0);
        var item = TieredBookingRecord.NewTieredBookingRecord("x", "y");
        item.Rate = 958.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredBookingRecordWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new TieredBookingRecordWeightRangeRule(7.2, 4019.7);
        var item = TieredBookingRecord.NewTieredBookingRecord("x", "y");
        item.Weight = 4021.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredBookingRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredBookingRecordRequiredCodeRule("tiered booking record needs this field");
        var item = TieredBookingRecord.NewTieredBookingRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredBookingRecordLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new TieredBookingRecordLocaleLengthRule(1000);
        var item = TieredBookingRecord.NewTieredBookingRecord("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredBookingRecordValidatorReportsEveryProblem();
        TieredBookingRecordRateRangeRuleFlagsValuesAboveRate();
        TieredBookingRecordWeightRangeRuleFlagsValuesAboveWeight();
        TieredBookingRecordRequiredCodeRuleFlagsABlankCode();
        TieredBookingRecordLocaleLengthRuleFlagsALongLocale();
    }
}
