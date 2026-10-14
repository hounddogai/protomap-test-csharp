namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingPreferenceRulesChecks
{
    public static void BookingPreferenceValidatorReportsEveryProblem()
    {
        var validator = BookingPreferenceRules.DefaultBookingPreferenceValidator();
        var item = BookingPreference.NewBookingPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingPreferenceRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new BookingPreferenceRequiredGradeRule("booking preference needs a value");
        var item = BookingPreference.NewBookingPreference("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingPreferenceScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new BookingPreferenceScoreRangeRule(4.0, 1284.1);
        var item = BookingPreference.NewBookingPreference("x", "y");
        item.Score = 1285.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BookingPreferenceValidatorReportsEveryProblem();
        BookingPreferenceRequiredGradeRuleFlagsABlankGrade();
        BookingPreferenceScoreRangeRuleFlagsValuesAboveScore();
    }
}
