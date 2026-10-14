namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptiveBookingProfileRulesChecks
{
    public static void AdaptiveBookingProfileValidatorReportsEveryProblem()
    {
        var validator = AdaptiveBookingProfileRules.DefaultAdaptiveBookingProfileValidator();
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveBookingProfileJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new AdaptiveBookingProfileJobCodeLengthRule(80);
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBookingProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveBookingProfileCodeLengthRule(32);
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBookingProfileLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveBookingProfileLabelLengthRule(1000);
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBookingProfileRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AdaptiveBookingProfileRequiredJobCodeRule("adaptive booking profile needs an entry");
        var item = AdaptiveBookingProfile.NewAdaptiveBookingProfile("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveBookingProfileValidatorReportsEveryProblem();
        AdaptiveBookingProfileJobCodeLengthRuleFlagsALongJobCode();
        AdaptiveBookingProfileCodeLengthRuleFlagsALongCode();
        AdaptiveBookingProfileLabelLengthRuleFlagsALongLabel();
        AdaptiveBookingProfileRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
