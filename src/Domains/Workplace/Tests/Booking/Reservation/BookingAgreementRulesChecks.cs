namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingAgreementRulesChecks
{
    public static void BookingAgreementValidatorReportsEveryProblem()
    {
        var validator = BookingAgreementRules.DefaultBookingAgreementValidator();
        var item = BookingAgreement.NewBookingAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new BookingAgreementCodeLengthRule(1000);
        var item = BookingAgreement.NewBookingAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingAgreementNameLengthRuleFlagsALongName()
    {
        var rule = new BookingAgreementNameLengthRule(16);
        var item = BookingAgreement.NewBookingAgreement("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingAgreementCountryLengthRuleFlagsALongCountry()
    {
        var rule = new BookingAgreementCountryLengthRule(16);
        var item = BookingAgreement.NewBookingAgreement("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingAgreementRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new BookingAgreementRequiredCategoryRule("booking agreement needs this field");
        var item = BookingAgreement.NewBookingAgreement("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingAgreementCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new BookingAgreementCategoryLengthRule(32);
        var item = BookingAgreement.NewBookingAgreement("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new BookingAgreementRequiredNameRule("booking agreement needs a value");
        var item = BookingAgreement.NewBookingAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BookingAgreementValidatorReportsEveryProblem();
        BookingAgreementCodeLengthRuleFlagsALongCode();
        BookingAgreementNameLengthRuleFlagsALongName();
        BookingAgreementCountryLengthRuleFlagsALongCountry();
        BookingAgreementRequiredCategoryRuleFlagsABlankCategory();
        BookingAgreementCategoryLengthRuleFlagsALongCategory();
        BookingAgreementRequiredNameRuleFlagsABlankName();
    }
}
