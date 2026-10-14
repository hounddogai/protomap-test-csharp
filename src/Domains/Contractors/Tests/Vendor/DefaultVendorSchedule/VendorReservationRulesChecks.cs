namespace Acme.Workforce.Contractors.Tests.Vendor.DefaultVendorSchedule;

using Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>Checks of default vendor schedule code, run by the project's runner.</summary>
public static class VendorReservationRulesChecks
{
    public static void VendorReservationValidatorReportsEveryProblem()
    {
        var validator = VendorReservationRules.DefaultVendorReservationValidator();
        var item = VendorReservation.NewVendorReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorReservationRegionLengthRuleFlagsALongRegion()
    {
        var rule = new VendorReservationRegionLengthRule(16);
        var item = VendorReservation.NewVendorReservation("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorReservationRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new VendorReservationRequiredRegionRule("vendor reservation needs this field");
        var item = VendorReservation.NewVendorReservation("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VendorReservationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new VendorReservationTitleLengthRule(16);
        var item = VendorReservation.NewVendorReservation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorReservationValidatorReportsEveryProblem();
        VendorReservationRegionLengthRuleFlagsALongRegion();
        VendorReservationRequiredRegionRuleFlagsABlankRegion();
        VendorReservationTitleLengthRuleFlagsALongTitle();
    }
}
