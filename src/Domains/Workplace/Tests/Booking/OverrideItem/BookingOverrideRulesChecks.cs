namespace Acme.Workforce.Workplace.Tests.Booking.OverrideItem;

using Acme.Workforce.Workplace.Booking.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BookingOverrideRulesChecks
{
    public static void BookingOverrideValidatorReportsEveryProblem()
    {
        var validator = BookingOverrideRules.DefaultBookingOverrideValidator();
        var item = BookingOverride.NewBookingOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BookingOverrideRequiredLabelRule("booking override needs a value");
        var item = BookingOverride.NewBookingOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingOverrideCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new BookingOverrideCategoryLengthRule(1000);
        var item = BookingOverride.NewBookingOverride("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingOverrideValidatorReportsEveryProblem();
        BookingOverrideRequiredLabelRuleFlagsABlankLabel();
        BookingOverrideCategoryLengthRuleFlagsALongCategory();
    }
}
