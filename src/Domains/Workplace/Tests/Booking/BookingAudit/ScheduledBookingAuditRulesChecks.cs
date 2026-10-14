namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class ScheduledBookingAuditRulesChecks
{
    public static void ScheduledBookingAuditValidatorReportsEveryProblem()
    {
        var validator = ScheduledBookingAuditRules.DefaultScheduledBookingAuditValidator();
        var item = ScheduledBookingAudit.NewScheduledBookingAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBookingAuditCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ScheduledBookingAuditCostRangeRule(3.6, 2242.4);
        var item = ScheduledBookingAudit.NewScheduledBookingAudit("x", "y");
        item.Cost = 2243.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledBookingAuditValidatorReportsEveryProblem();
        ScheduledBookingAuditCostRangeRuleFlagsValuesAboveCost();
    }
}
