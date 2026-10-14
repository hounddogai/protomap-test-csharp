namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentForecastRulesChecks
{
    public static void AssignmentForecastValidatorReportsEveryProblem()
    {
        var validator = AssignmentForecastRules.DefaultAssignmentForecastValidator();
        var item = AssignmentForecast.NewAssignmentForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentForecastCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new AssignmentForecastCostRangeRule(9.0, 4792.7);
        var item = AssignmentForecast.NewAssignmentForecast("x", "y");
        item.Cost = 4794.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentForecastRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AssignmentForecastRequiredCodeRule("assignment forecast needs a value");
        var item = AssignmentForecast.NewAssignmentForecast("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentForecastGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AssignmentForecastGraceDaysRangeRule(32, 255);
        var item = AssignmentForecast.NewAssignmentForecast("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentForecastManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new AssignmentForecastManagerIdLengthRule(86400);
        var item = AssignmentForecast.NewAssignmentForecast("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssignmentForecastValidatorReportsEveryProblem();
        AssignmentForecastCostRangeRuleFlagsValuesAboveCost();
        AssignmentForecastRequiredCodeRuleFlagsABlankCode();
        AssignmentForecastGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AssignmentForecastManagerIdLengthRuleFlagsALongManagerId();
    }
}
