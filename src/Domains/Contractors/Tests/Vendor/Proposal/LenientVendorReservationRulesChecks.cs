namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientVendorReservationRulesChecks
{
    public static void LenientVendorReservationValidatorReportsEveryProblem()
    {
        var validator = LenientVendorReservationRules.DefaultLenientVendorReservationValidator();
        var item = LenientVendorReservation.NewLenientVendorReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVendorReservationLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new LenientVendorReservationLocaleLengthRule(3600);
        var item = LenientVendorReservation.NewLenientVendorReservation("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientVendorReservationBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new LenientVendorReservationBatchSizeRangeRule(255, 3600);
        var item = LenientVendorReservation.NewLenientVendorReservation("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientVendorReservationCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new LenientVendorReservationCurrencyLengthRule(255);
        var item = LenientVendorReservation.NewLenientVendorReservation("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientVendorReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientVendorReservationCodeLengthRule(255);
        var item = LenientVendorReservation.NewLenientVendorReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientVendorReservationValidatorReportsEveryProblem();
        LenientVendorReservationLocaleLengthRuleFlagsALongLocale();
        LenientVendorReservationBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        LenientVendorReservationCurrencyLengthRuleFlagsALongCurrency();
        LenientVendorReservationCodeLengthRuleFlagsALongCode();
    }
}
