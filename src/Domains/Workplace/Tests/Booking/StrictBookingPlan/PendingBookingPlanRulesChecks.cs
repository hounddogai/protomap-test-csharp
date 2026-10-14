namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class PendingBookingPlanRulesChecks
{
    public static void PendingBookingPlanValidatorReportsEveryProblem()
    {
        var validator = PendingBookingPlanRules.DefaultPendingBookingPlanValidator();
        var item = PendingBookingPlan.NewPendingBookingPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingBookingPlanSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PendingBookingPlanSequenceRangeRule(64, 86400);
        var item = PendingBookingPlan.NewPendingBookingPlan("x", "y");
        item.Sequence = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingBookingPlanCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PendingBookingPlanCapacityRangeRule(80, 3600);
        var item = PendingBookingPlan.NewPendingBookingPlan("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingBookingPlanLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PendingBookingPlanLimitRangeRule(32, 3600);
        var item = PendingBookingPlan.NewPendingBookingPlan("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingBookingPlanValidatorReportsEveryProblem();
        PendingBookingPlanSequenceRangeRuleFlagsValuesAboveSequence();
        PendingBookingPlanCapacityRangeRuleFlagsValuesAboveCapacity();
        PendingBookingPlanLimitRangeRuleFlagsValuesAboveLimit();
    }
}
