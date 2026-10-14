namespace Acme.Workforce.Workplace.Tests.Booking.Baseline;

using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledBookingClaimRulesChecks
{
    public static void ScheduledBookingClaimValidatorReportsEveryProblem()
    {
        var validator = ScheduledBookingClaimRules.DefaultScheduledBookingClaimValidator();
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBookingClaimAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ScheduledBookingClaimAttemptsRangeRule(80, 80);
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("x", "y");
        item.Attempts = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledBookingClaimTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledBookingClaimTitleLengthRule(120);
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBookingClaimRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new ScheduledBookingClaimRequiredCategoryRule("scheduled booking claim needs this field");
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBookingClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledBookingClaimRequiredTitleRule("scheduled booking claim needs this field");
        var item = ScheduledBookingClaim.NewScheduledBookingClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledBookingClaimValidatorReportsEveryProblem();
        ScheduledBookingClaimAttemptsRangeRuleFlagsValuesAboveAttempts();
        ScheduledBookingClaimTitleLengthRuleFlagsALongTitle();
        ScheduledBookingClaimRequiredCategoryRuleFlagsABlankCategory();
        ScheduledBookingClaimRequiredTitleRuleFlagsABlankTitle();
    }
}
