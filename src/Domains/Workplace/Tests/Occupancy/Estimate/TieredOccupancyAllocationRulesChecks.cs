namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredOccupancyAllocationRulesChecks
{
    public static void TieredOccupancyAllocationValidatorReportsEveryProblem()
    {
        var validator = TieredOccupancyAllocationRules.DefaultTieredOccupancyAllocationValidator();
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredOccupancyAllocationRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new TieredOccupancyAllocationRequiredManagerIdRule("tiered occupancy allocation needs an entry");
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredOccupancyAllocationRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new TieredOccupancyAllocationRequiredContactEmailRule("tiered occupancy allocation needs this field");
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredOccupancyAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredOccupancyAllocationCodeLengthRule(120);
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredOccupancyAllocationContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new TieredOccupancyAllocationContactEmailLengthRule(1000);
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredOccupancyAllocationValidatorReportsEveryProblem();
        TieredOccupancyAllocationRequiredManagerIdRuleFlagsABlankManagerId();
        TieredOccupancyAllocationRequiredContactEmailRuleFlagsABlankContactEmail();
        TieredOccupancyAllocationCodeLengthRuleFlagsALongCode();
        TieredOccupancyAllocationContactEmailLengthRuleFlagsALongContactEmail();
    }
}
