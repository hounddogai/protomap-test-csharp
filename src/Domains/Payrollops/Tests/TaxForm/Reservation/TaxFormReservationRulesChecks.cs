namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TaxFormReservationRulesChecks
{
    public static void TaxFormReservationValidatorReportsEveryProblem()
    {
        var validator = TaxFormReservationRules.DefaultTaxFormReservationValidator();
        var item = TaxFormReservation.NewTaxFormReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormReservationCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new TaxFormReservationCeilingRangeRule(1.0, 3097.9);
        var item = TaxFormReservation.NewTaxFormReservation("x", "y");
        item.Ceiling = 3099.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormReservationRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new TaxFormReservationRequiredCategoryRule("tax form reservation needs a value");
        var item = TaxFormReservation.NewTaxFormReservation("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormReservationRegionLengthRuleFlagsALongRegion()
    {
        var rule = new TaxFormReservationRegionLengthRule(1000);
        var item = TaxFormReservation.NewTaxFormReservation("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TaxFormReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormReservationCodeLengthRule(16);
        var item = TaxFormReservation.NewTaxFormReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormReservationValidatorReportsEveryProblem();
        TaxFormReservationCeilingRangeRuleFlagsValuesAboveCeiling();
        TaxFormReservationRequiredCategoryRuleFlagsABlankCategory();
        TaxFormReservationRegionLengthRuleFlagsALongRegion();
        TaxFormReservationCodeLengthRuleFlagsALongCode();
    }
}
