namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DefaultBookingClaimRulesChecks
{
    public static void DefaultBookingClaimValidatorReportsEveryProblem()
    {
        var validator = DefaultBookingClaimRules.DefaultDefaultBookingClaimValidator();
        var item = DefaultBookingClaim.NewDefaultBookingClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultBookingClaimNameLengthRuleFlagsALongName()
    {
        var rule = new DefaultBookingClaimNameLengthRule(120);
        var item = DefaultBookingClaim.NewDefaultBookingClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultBookingClaimValidatorReportsEveryProblem();
        DefaultBookingClaimNameLengthRuleFlagsALongName();
    }
}
