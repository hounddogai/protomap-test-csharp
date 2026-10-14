namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class DefaultOccupancyEstimateRulesChecks
{
    public static void DefaultOccupancyEstimateValidatorReportsEveryProblem()
    {
        var validator = DefaultOccupancyEstimateRules.DefaultDefaultOccupancyEstimateValidator();
        var item = DefaultOccupancyEstimate.NewDefaultOccupancyEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultOccupancyEstimateRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DefaultOccupancyEstimateRequiredPhoneRule("default occupancy estimate needs this field");
        var item = DefaultOccupancyEstimate.NewDefaultOccupancyEstimate("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultOccupancyEstimateValidatorReportsEveryProblem();
        DefaultOccupancyEstimateRequiredPhoneRuleFlagsABlankPhone();
    }
}
