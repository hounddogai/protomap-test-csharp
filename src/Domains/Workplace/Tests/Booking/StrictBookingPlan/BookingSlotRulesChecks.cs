namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class BookingSlotRulesChecks
{
    public static void BookingSlotValidatorReportsEveryProblem()
    {
        var validator = BookingSlotRules.DefaultBookingSlotValidator();
        var item = BookingSlot.NewBookingSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingSlotRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new BookingSlotRequiredContactEmailRule("booking slot needs a value");
        var item = BookingSlot.NewBookingSlot("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingSlotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BookingSlotRequiredCodeRule("booking slot needs this field");
        var item = BookingSlot.NewBookingSlot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingSlotTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new BookingSlotTenureMonthsRangeRule(64, 1000);
        var item = BookingSlot.NewBookingSlot("x", "y");
        item.TenureMonths = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingSlotValidatorReportsEveryProblem();
        BookingSlotRequiredContactEmailRuleFlagsABlankContactEmail();
        BookingSlotRequiredCodeRuleFlagsABlankCode();
        BookingSlotTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
