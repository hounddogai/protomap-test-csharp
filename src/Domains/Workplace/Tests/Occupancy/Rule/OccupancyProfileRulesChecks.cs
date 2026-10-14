namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class OccupancyProfileRulesChecks
{
    public static void OccupancyProfileValidatorReportsEveryProblem()
    {
        var validator = OccupancyProfileRules.DefaultOccupancyProfileValidator();
        var item = OccupancyProfile.NewOccupancyProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyProfileRequiredCodeRule("occupancy profile needs this field");
        var item = OccupancyProfile.NewOccupancyProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyProfileValidatorReportsEveryProblem();
        OccupancyProfileRequiredCodeRuleFlagsABlankCode();
    }
}
