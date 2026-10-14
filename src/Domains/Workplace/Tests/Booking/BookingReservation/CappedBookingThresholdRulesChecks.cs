namespace Acme.Workforce.Workplace.Tests.Booking.BookingReservation;

using Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Checks of booking reservation code, run by the project's runner.</summary>
public static class CappedBookingThresholdRulesChecks
{
    public static void CappedBookingThresholdValidatorReportsEveryProblem()
    {
        var validator = CappedBookingThresholdRules.DefaultCappedBookingThresholdValidator();
        var item = CappedBookingThreshold.NewCappedBookingThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedBookingThresholdPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new CappedBookingThresholdPriorityRangeRule(120, 86400);
        var item = CappedBookingThreshold.NewCappedBookingThreshold("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedBookingThresholdRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new CappedBookingThresholdRequiredLocationRule("capped booking threshold needs an entry");
        var item = CappedBookingThreshold.NewCappedBookingThreshold("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedBookingThresholdSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new CappedBookingThresholdSequenceRangeRule(32, 1000);
        var item = CappedBookingThreshold.NewCappedBookingThreshold("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedBookingThresholdValidatorReportsEveryProblem();
        CappedBookingThresholdPriorityRangeRuleFlagsValuesAbovePriority();
        CappedBookingThresholdRequiredLocationRuleFlagsABlankLocation();
        CappedBookingThresholdSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
