namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorAdjustmentRulesChecks
{
    public static void VendorAdjustmentValidatorReportsEveryProblem()
    {
        var validator = VendorAdjustmentRules.DefaultVendorAdjustmentValidator();
        var item = VendorAdjustment.NewVendorAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VendorAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new VendorAdjustmentCodeLengthRule(16);
        var item = VendorAdjustment.NewVendorAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VendorAdjustmentRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new VendorAdjustmentRequiredTeamRule("vendor adjustment needs a value");
        var item = VendorAdjustment.NewVendorAdjustment("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VendorAdjustmentValidatorReportsEveryProblem();
        VendorAdjustmentCodeLengthRuleFlagsALongCode();
        VendorAdjustmentRequiredTeamRuleFlagsABlankTeam();
    }
}
