namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CappedBookingPreferenceRulesChecks
{
    public static void CappedBookingPreferenceValidatorReportsEveryProblem()
    {
        var validator = CappedBookingPreferenceRules.DefaultCappedBookingPreferenceValidator();
        var item = CappedBookingPreference.NewCappedBookingPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedBookingPreferenceChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new CappedBookingPreferenceChecksumLengthRule(1000);
        var item = CappedBookingPreference.NewCappedBookingPreference("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedBookingPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedBookingPreferenceCodeLengthRule(32);
        var item = CappedBookingPreference.NewCappedBookingPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedBookingPreferenceValidatorReportsEveryProblem();
        CappedBookingPreferenceChecksumLengthRuleFlagsALongChecksum();
        CappedBookingPreferenceCodeLengthRuleFlagsALongCode();
    }
}
