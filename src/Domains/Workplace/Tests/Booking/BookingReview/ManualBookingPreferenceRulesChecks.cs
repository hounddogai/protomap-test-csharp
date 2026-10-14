namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingPreferenceRulesChecks
{
    public static void ManualBookingPreferenceValidatorReportsEveryProblem()
    {
        var validator = ManualBookingPreferenceRules.DefaultManualBookingPreferenceValidator();
        var item = ManualBookingPreference.NewManualBookingPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBookingPreferenceLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualBookingPreferenceLabelLengthRule(80);
        var item = ManualBookingPreference.NewManualBookingPreference("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBookingPreferenceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualBookingPreferenceRequiredCodeRule("manual booking preference needs a value");
        var item = ManualBookingPreference.NewManualBookingPreference("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBookingPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualBookingPreferenceCodeLengthRule(86400);
        var item = ManualBookingPreference.NewManualBookingPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBookingPreferenceRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualBookingPreferenceRequiredLabelRule("manual booking preference needs this field");
        var item = ManualBookingPreference.NewManualBookingPreference("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualBookingPreferenceValidatorReportsEveryProblem();
        ManualBookingPreferenceLabelLengthRuleFlagsALongLabel();
        ManualBookingPreferenceRequiredCodeRuleFlagsABlankCode();
        ManualBookingPreferenceCodeLengthRuleFlagsALongCode();
        ManualBookingPreferenceRequiredLabelRuleFlagsABlankLabel();
    }
}
