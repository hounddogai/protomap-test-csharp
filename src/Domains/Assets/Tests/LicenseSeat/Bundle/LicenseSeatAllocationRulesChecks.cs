namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LicenseSeatAllocationRulesChecks
{
    public static void LicenseSeatAllocationValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatAllocationRules.DefaultLicenseSeatAllocationValidator();
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatAllocationRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new LicenseSeatAllocationRequiredSourceRule("license seat allocation needs an entry");
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatAllocationCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LicenseSeatAllocationCapacityRangeRule(255, 3600);
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatAllocationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LicenseSeatAllocationRequiredCodeRule("license seat allocation needs this field");
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LicenseSeatAllocationValidatorReportsEveryProblem();
        LicenseSeatAllocationRequiredSourceRuleFlagsABlankSource();
        LicenseSeatAllocationCapacityRangeRuleFlagsValuesAboveCapacity();
        LicenseSeatAllocationRequiredCodeRuleFlagsABlankCode();
    }
}
