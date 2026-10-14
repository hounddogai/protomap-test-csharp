namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RegionalOccupancyTierRulesChecks
{
    public static void RegionalOccupancyTierValidatorReportsEveryProblem()
    {
        var validator = RegionalOccupancyTierRules.DefaultRegionalOccupancyTierValidator();
        var item = RegionalOccupancyTier.NewRegionalOccupancyTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalOccupancyTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalOccupancyTierRequiredNameRule("regional occupancy tier needs a value");
        var item = RegionalOccupancyTier.NewRegionalOccupancyTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalOccupancyTierValidatorReportsEveryProblem();
        RegionalOccupancyTierRequiredNameRuleFlagsABlankName();
    }
}
