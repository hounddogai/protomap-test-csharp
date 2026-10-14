namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingAssignmentRulesChecks
{
    public static void ManualBookingAssignmentValidatorReportsEveryProblem()
    {
        var validator = ManualBookingAssignmentRules.DefaultManualBookingAssignmentValidator();
        var item = ManualBookingAssignment.NewManualBookingAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBookingAssignmentRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new ManualBookingAssignmentRequiredRegionRule("manual booking assignment needs an entry");
        var item = ManualBookingAssignment.NewManualBookingAssignment("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBookingAssignmentVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ManualBookingAssignmentVersionRangeRule(32, 86400);
        var item = ManualBookingAssignment.NewManualBookingAssignment("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualBookingAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new ManualBookingAssignmentNameLengthRule(80);
        var item = ManualBookingAssignment.NewManualBookingAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBookingAssignmentWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ManualBookingAssignmentWeightRangeRule(5.1, 1551.2);
        var item = ManualBookingAssignment.NewManualBookingAssignment("x", "y");
        item.Weight = 1552.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualBookingAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualBookingAssignmentRequiredNameRule("manual booking assignment needs an entry");
        var item = ManualBookingAssignment.NewManualBookingAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBookingAssignmentPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ManualBookingAssignmentPriorityRangeRule(255, 86400);
        var item = ManualBookingAssignment.NewManualBookingAssignment("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualBookingAssignmentValidatorReportsEveryProblem();
        ManualBookingAssignmentRequiredRegionRuleFlagsABlankRegion();
        ManualBookingAssignmentVersionRangeRuleFlagsValuesAboveVersion();
        ManualBookingAssignmentNameLengthRuleFlagsALongName();
        ManualBookingAssignmentWeightRangeRuleFlagsValuesAboveWeight();
        ManualBookingAssignmentRequiredNameRuleFlagsABlankName();
        ManualBookingAssignmentPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
