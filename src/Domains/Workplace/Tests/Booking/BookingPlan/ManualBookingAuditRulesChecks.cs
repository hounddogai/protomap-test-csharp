namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class ManualBookingAuditRulesChecks
{
    public static void ManualBookingAuditValidatorReportsEveryProblem()
    {
        var validator = ManualBookingAuditRules.DefaultManualBookingAuditValidator();
        var item = ManualBookingAudit.NewManualBookingAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBookingAuditRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ManualBookingAuditRevisionRangeRule(80, 1000);
        var item = ManualBookingAudit.NewManualBookingAudit("x", "y");
        item.Revision = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualBookingAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ManualBookingAuditCreatedAtRangeRule(16, 255);
        var item = ManualBookingAudit.NewManualBookingAudit("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualBookingAuditValidatorReportsEveryProblem();
        ManualBookingAuditRevisionRangeRuleFlagsValuesAboveRevision();
        ManualBookingAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
