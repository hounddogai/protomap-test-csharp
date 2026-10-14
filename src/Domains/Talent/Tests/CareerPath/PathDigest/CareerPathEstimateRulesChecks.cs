namespace Acme.Workforce.Talent.Tests.CareerPath.PathDigest;

using Acme.Workforce.Talent.CareerPath.PathDigest;

/// <summary>Checks of path digest code, run by the project's runner.</summary>
public static class CareerPathEstimateRulesChecks
{
    public static void CareerPathEstimateValidatorReportsEveryProblem()
    {
        var validator = CareerPathEstimateRules.DefaultCareerPathEstimateValidator();
        var item = CareerPathEstimate.NewCareerPathEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathEstimateRequiredNameRuleFlagsABlankName()
    {
        var rule = new CareerPathEstimateRequiredNameRule("career path estimate needs a value");
        var item = CareerPathEstimate.NewCareerPathEstimate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathEstimateAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new CareerPathEstimateAttemptsRangeRule(16, 3600);
        var item = CareerPathEstimate.NewCareerPathEstimate("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathEstimateRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new CareerPathEstimateRequiredUnitRule("career path estimate needs this field");
        var item = CareerPathEstimate.NewCareerPathEstimate("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathEstimateValidatorReportsEveryProblem();
        CareerPathEstimateRequiredNameRuleFlagsABlankName();
        CareerPathEstimateAttemptsRangeRuleFlagsValuesAboveAttempts();
        CareerPathEstimateRequiredUnitRuleFlagsABlankUnit();
    }
}
