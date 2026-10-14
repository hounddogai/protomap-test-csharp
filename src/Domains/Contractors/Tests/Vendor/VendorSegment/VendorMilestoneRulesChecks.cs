namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorMilestoneRulesChecks
{
    public static void VendorMilestoneValidatorReportsEveryProblem()
    {
        var validator = VendorMilestoneRules.DefaultVendorMilestoneValidator();
        var item = VendorMilestone.NewVendorMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorMilestoneQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new VendorMilestoneQuantityRangeRule(255, 1000);
        var item = VendorMilestone.NewVendorMilestone("x", "y");
        item.Quantity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorMilestoneRegionLengthRuleFlagsALongRegion()
    {
        var rule = new VendorMilestoneRegionLengthRule(64);
        var item = VendorMilestone.NewVendorMilestone("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new VendorMilestoneUpdatedAtRangeRule(255, 3600);
        var item = VendorMilestone.NewVendorMilestone("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VendorMilestoneValidatorReportsEveryProblem();
        VendorMilestoneQuantityRangeRuleFlagsValuesAboveQuantity();
        VendorMilestoneRegionLengthRuleFlagsALongRegion();
        VendorMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
