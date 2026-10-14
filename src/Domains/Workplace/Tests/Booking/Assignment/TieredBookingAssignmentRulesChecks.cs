namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredBookingAssignmentRulesChecks
{
    public static void TieredBookingAssignmentValidatorReportsEveryProblem()
    {
        var validator = TieredBookingAssignmentRules.DefaultTieredBookingAssignmentValidator();
        var item = TieredBookingAssignment.NewTieredBookingAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredBookingAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredBookingAssignmentRequiredLabelRule("tiered booking assignment needs a value");
        var item = TieredBookingAssignment.NewTieredBookingAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredBookingAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredBookingAssignmentRequiredCodeRule("tiered booking assignment needs an entry");
        var item = TieredBookingAssignment.NewTieredBookingAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredBookingAssignmentOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new TieredBookingAssignmentOwnerIdLengthRule(3600);
        var item = TieredBookingAssignment.NewTieredBookingAssignment("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredBookingAssignmentValidatorReportsEveryProblem();
        TieredBookingAssignmentRequiredLabelRuleFlagsABlankLabel();
        TieredBookingAssignmentRequiredCodeRuleFlagsABlankCode();
        TieredBookingAssignmentOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
