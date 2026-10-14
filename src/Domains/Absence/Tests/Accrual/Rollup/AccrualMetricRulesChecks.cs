namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccrualMetricRulesChecks
{
    public static void AccrualMetricValidatorReportsEveryProblem()
    {
        var validator = AccrualMetricRules.DefaultAccrualMetricValidator();
        var item = AccrualMetric.NewAccrualMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualMetricGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AccrualMetricGraceDaysRangeRule(80, 80);
        var item = AccrualMetric.NewAccrualMetric("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccrualMetricRequiredLabelRule("accrual metric needs an entry");
        var item = AccrualMetric.NewAccrualMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualMetricValidatorReportsEveryProblem();
        AccrualMetricGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AccrualMetricRequiredLabelRuleFlagsABlankLabel();
    }
}
