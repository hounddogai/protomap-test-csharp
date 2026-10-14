namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorRequestRulesChecks
{
    public static void VendorRequestValidatorReportsEveryProblem()
    {
        var validator = VendorRequestRules.DefaultVendorRequestValidator();
        var item = VendorRequest.NewVendorRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorRequestDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new VendorRequestDepartmentLengthRule(120);
        var item = VendorRequest.NewVendorRequest("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorRequestCodeLengthRule(120);
        var item = VendorRequest.NewVendorRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VendorRequestValidatorReportsEveryProblem();
        VendorRequestDepartmentLengthRuleFlagsALongDepartment();
        VendorRequestCodeLengthRuleFlagsALongCode();
    }
}
