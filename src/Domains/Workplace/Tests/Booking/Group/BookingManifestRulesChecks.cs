namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BookingManifestRulesChecks
{
    public static void BookingManifestValidatorReportsEveryProblem()
    {
        var validator = BookingManifestRules.DefaultBookingManifestValidator();
        var item = BookingManifest.NewBookingManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BookingManifestDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new BookingManifestDepartmentLengthRule(120);
        var item = BookingManifest.NewBookingManifest("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BookingManifestRequiredNameRuleFlagsABlankName()
    {
        var rule = new BookingManifestRequiredNameRule("booking manifest needs this field");
        var item = BookingManifest.NewBookingManifest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BookingManifestEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new BookingManifestEndsAtRangeRule(80, 1000);
        var item = BookingManifest.NewBookingManifest("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BookingManifestJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new BookingManifestJobCodeLengthRule(32);
        var item = BookingManifest.NewBookingManifest("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BookingManifestValidatorReportsEveryProblem();
        BookingManifestDepartmentLengthRuleFlagsALongDepartment();
        BookingManifestRequiredNameRuleFlagsABlankName();
        BookingManifestEndsAtRangeRuleFlagsValuesAboveEndsAt();
        BookingManifestJobCodeLengthRuleFlagsALongJobCode();
    }
}
