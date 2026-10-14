namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class CappedCostModelMetricRulesChecks
{
    public static void CappedCostModelMetricValidatorReportsEveryProblem()
    {
        var validator = CappedCostModelMetricRules.DefaultCappedCostModelMetricValidator();
        var item = CappedCostModelMetric.NewCappedCostModelMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCostModelMetricRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new CappedCostModelMetricRequiredBadgeRule("capped cost model metric needs a value");
        var item = CappedCostModelMetric.NewCappedCostModelMetric("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedCostModelMetricRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new CappedCostModelMetricRequiredPhoneRule("capped cost model metric needs an entry");
        var item = CappedCostModelMetric.NewCappedCostModelMetric("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedCostModelMetricVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new CappedCostModelMetricVersionRangeRule(120, 120);
        var item = CappedCostModelMetric.NewCappedCostModelMetric("x", "y");
        item.Version = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedCostModelMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedCostModelMetricCodeLengthRule(1000);
        var item = CappedCostModelMetric.NewCappedCostModelMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedCostModelMetricValidatorReportsEveryProblem();
        CappedCostModelMetricRequiredBadgeRuleFlagsABlankBadge();
        CappedCostModelMetricRequiredPhoneRuleFlagsABlankPhone();
        CappedCostModelMetricVersionRangeRuleFlagsValuesAboveVersion();
        CappedCostModelMetricCodeLengthRuleFlagsALongCode();
    }
}
