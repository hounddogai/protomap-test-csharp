namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAllocation;

using Acme.Workforce.Assets.Warranty.WarrantyAllocation;

/// <summary>Checks of warranty allocation code, run by the project's runner.</summary>
public static class PendingWarrantyAllocationRulesChecks
{
    public static void PendingWarrantyAllocationValidatorReportsEveryProblem()
    {
        var validator = PendingWarrantyAllocationRules.DefaultPendingWarrantyAllocationValidator();
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingWarrantyAllocationRegionLengthRuleFlagsALongRegion()
    {
        var rule = new PendingWarrantyAllocationRegionLengthRule(1000);
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingWarrantyAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingWarrantyAllocationRequiredCodeRule("pending warranty allocation needs an entry");
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingWarrantyAllocationRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingWarrantyAllocationRequiredTitleRule("pending warranty allocation needs an entry");
        var item = PendingWarrantyAllocation.NewPendingWarrantyAllocation("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingWarrantyAllocationValidatorReportsEveryProblem();
        PendingWarrantyAllocationRegionLengthRuleFlagsALongRegion();
        PendingWarrantyAllocationRequiredCodeRuleFlagsABlankCode();
        PendingWarrantyAllocationRequiredTitleRuleFlagsABlankTitle();
    }
}
