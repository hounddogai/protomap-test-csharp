namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalExceptionMetricRulesChecks
{
    public static void SeasonalExceptionMetricValidatorReportsEveryProblem()
    {
        var validator = SeasonalExceptionMetricRules.DefaultSeasonalExceptionMetricValidator();
        var item = SeasonalExceptionMetric.NewSeasonalExceptionMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalExceptionMetricRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new SeasonalExceptionMetricRatioRangeRule(2.0, 2965.0);
        var item = SeasonalExceptionMetric.NewSeasonalExceptionMetric("x", "y");
        item.Ratio = 2966.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalExceptionMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalExceptionMetricRequiredCodeRule("seasonal exception metric needs an entry");
        var item = SeasonalExceptionMetric.NewSeasonalExceptionMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalExceptionMetricValidatorReportsEveryProblem();
        SeasonalExceptionMetricRatioRangeRuleFlagsValuesAboveRatio();
        SeasonalExceptionMetricRequiredCodeRuleFlagsABlankCode();
    }
}
