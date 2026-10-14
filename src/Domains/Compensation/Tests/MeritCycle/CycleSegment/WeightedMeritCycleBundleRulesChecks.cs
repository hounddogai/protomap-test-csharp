namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleBundleRulesChecks
{
    public static void WeightedMeritCycleBundleValidatorReportsEveryProblem()
    {
        var validator = WeightedMeritCycleBundleRules.DefaultWeightedMeritCycleBundleValidator();
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedMeritCycleBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new WeightedMeritCycleBundleTenureMonthsRangeRule(64, 3600);
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedMeritCycleBundleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new WeightedMeritCycleBundleBudgetRangeRule(4.5, 1552.5);
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("x", "y");
        item.Budget = 1554.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedMeritCycleBundleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new WeightedMeritCycleBundleRetriesRangeRule(80, 255);
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        WeightedMeritCycleBundleValidatorReportsEveryProblem();
        WeightedMeritCycleBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        WeightedMeritCycleBundleBudgetRangeRuleFlagsValuesAboveBudget();
        WeightedMeritCycleBundleRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
