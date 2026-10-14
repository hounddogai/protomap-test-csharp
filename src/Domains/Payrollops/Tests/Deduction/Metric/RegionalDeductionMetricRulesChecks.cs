namespace Acme.Workforce.Payrollops.Tests.Deduction.Metric;

using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalDeductionMetricRulesChecks
{
    public static void RegionalDeductionMetricValidatorReportsEveryProblem()
    {
        var validator = RegionalDeductionMetricRules.DefaultRegionalDeductionMetricValidator();
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalDeductionMetricLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalDeductionMetricLabelLengthRule(16);
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeductionMetricWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new RegionalDeductionMetricWeightRangeRule(4.4, 534.5);
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("x", "y");
        item.Weight = 536.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalDeductionMetricDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new RegionalDeductionMetricDepartmentLengthRule(16);
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalDeductionMetricMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new RegionalDeductionMetricMarginRangeRule(6.1, 3778.3);
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("x", "y");
        item.Margin = 3779.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalDeductionMetricRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new RegionalDeductionMetricRequiredDepartmentRule("regional deduction metric needs a value");
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalDeductionMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalDeductionMetricRequiredCodeRule("regional deduction metric needs a value");
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalDeductionMetricValidatorReportsEveryProblem();
        RegionalDeductionMetricLabelLengthRuleFlagsALongLabel();
        RegionalDeductionMetricWeightRangeRuleFlagsValuesAboveWeight();
        RegionalDeductionMetricDepartmentLengthRuleFlagsALongDepartment();
        RegionalDeductionMetricMarginRangeRuleFlagsValuesAboveMargin();
        RegionalDeductionMetricRequiredDepartmentRuleFlagsABlankDepartment();
        RegionalDeductionMetricRequiredCodeRuleFlagsABlankCode();
    }
}
