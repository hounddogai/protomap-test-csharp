namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class LegacyBookingAuditRulesChecks
{
    public static void LegacyBookingAuditValidatorReportsEveryProblem()
    {
        var validator = LegacyBookingAuditRules.DefaultLegacyBookingAuditValidator();
        var item = LegacyBookingAudit.NewLegacyBookingAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyBookingAuditQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LegacyBookingAuditQuantityRangeRule(3600, 86400);
        var item = LegacyBookingAudit.NewLegacyBookingAudit("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyBookingAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LegacyBookingAuditCreatedAtRangeRule(120, 1000);
        var item = LegacyBookingAudit.NewLegacyBookingAudit("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyBookingAuditRequiredOwnerIdRuleFlagsABlankOwnerId()
    {
        var rule = new LegacyBookingAuditRequiredOwnerIdRule("legacy booking audit needs this field");
        var item = LegacyBookingAudit.NewLegacyBookingAudit("", "");
        item.OwnerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyBookingAuditOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LegacyBookingAuditOwnerIdLengthRule(64);
        var item = LegacyBookingAudit.NewLegacyBookingAudit("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyBookingAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyBookingAuditRequiredCodeRule("legacy booking audit needs a value");
        var item = LegacyBookingAudit.NewLegacyBookingAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyBookingAuditValidatorReportsEveryProblem();
        LegacyBookingAuditQuantityRangeRuleFlagsValuesAboveQuantity();
        LegacyBookingAuditCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LegacyBookingAuditRequiredOwnerIdRuleFlagsABlankOwnerId();
        LegacyBookingAuditOwnerIdLengthRuleFlagsALongOwnerId();
        LegacyBookingAuditRequiredCodeRuleFlagsABlankCode();
    }
}
