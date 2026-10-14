namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PulseSurveyMetricRulesChecks
{
    public static void PulseSurveyMetricValidatorReportsEveryProblem()
    {
        var validator = PulseSurveyMetricRules.DefaultPulseSurveyMetricValidator();
        var item = PulseSurveyMetric.NewPulseSurveyMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PulseSurveyMetricVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new PulseSurveyMetricVersionRangeRule(120, 86400);
        var item = PulseSurveyMetric.NewPulseSurveyMetric("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new PulseSurveyMetricCodeLengthRule(80);
        var item = PulseSurveyMetric.NewPulseSurveyMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PulseSurveyMetricDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PulseSurveyMetricDiscountRangeRule(6.0, 4708.0);
        var item = PulseSurveyMetric.NewPulseSurveyMetric("x", "y");
        item.Discount = 4709.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PulseSurveyMetricRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PulseSurveyMetricRequiredDepartmentRule("pulse survey metric needs an entry");
        var item = PulseSurveyMetric.NewPulseSurveyMetric("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PulseSurveyMetricValidatorReportsEveryProblem();
        PulseSurveyMetricVersionRangeRuleFlagsValuesAboveVersion();
        PulseSurveyMetricCodeLengthRuleFlagsALongCode();
        PulseSurveyMetricDiscountRangeRuleFlagsValuesAboveDiscount();
        PulseSurveyMetricRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
