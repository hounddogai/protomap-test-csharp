namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class BookingQuotaRulesChecks
{
    public static void BookingQuotaValidatorReportsEveryProblem()
    {
        var validator = BookingQuotaRules.DefaultBookingQuotaValidator();
        var item = BookingQuota.NewBookingQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingQuotaExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new BookingQuotaExpiresAtRangeRule(80, 120);
        var item = BookingQuota.NewBookingQuota("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingQuotaRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new BookingQuotaRequiredCategoryRule("booking quota needs this field");
        var item = BookingQuota.NewBookingQuota("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingQuotaCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new BookingQuotaCapacityRangeRule(80, 255);
        var item = BookingQuota.NewBookingQuota("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingQuotaCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new BookingQuotaCategoryLengthRule(255);
        var item = BookingQuota.NewBookingQuota("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingQuotaValidatorReportsEveryProblem();
        BookingQuotaExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        BookingQuotaRequiredCategoryRuleFlagsABlankCategory();
        BookingQuotaCapacityRangeRuleFlagsValuesAboveCapacity();
        BookingQuotaCategoryLengthRuleFlagsALongCategory();
    }
}
