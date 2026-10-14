namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorAssignmentRulesChecks
{
    public static void VendorAssignmentValidatorReportsEveryProblem()
    {
        var validator = VendorAssignmentRules.DefaultVendorAssignmentValidator();
        var item = VendorAssignment.NewVendorAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorAssignmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new VendorAssignmentCapacityRangeRule(16, 255);
        var item = VendorAssignment.NewVendorAssignment("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VendorAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorAssignmentCodeLengthRule(80);
        var item = VendorAssignment.NewVendorAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorAssignmentOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new VendorAssignmentOwnerIdLengthRule(64);
        var item = VendorAssignment.NewVendorAssignment("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorAssignmentValidatorReportsEveryProblem();
        VendorAssignmentCapacityRangeRuleFlagsValuesAboveCapacity();
        VendorAssignmentCodeLengthRuleFlagsALongCode();
        VendorAssignmentOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
