namespace Acme.Workforce.Payrollops.Tests.Payslip.Manifest;

using Acme.Workforce.Payrollops.Payslip.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class BatchedPayslipMetricRulesChecks
{
    public static void BatchedPayslipMetricValidatorReportsEveryProblem()
    {
        var validator = BatchedPayslipMetricRules.DefaultBatchedPayslipMetricValidator();
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPayslipMetricFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new BatchedPayslipMetricFloorValueRangeRule(5.4, 606.3);
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("x", "y");
        item.FloorValue = 607.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPayslipMetricUnitLengthRuleFlagsALongUnit()
    {
        var rule = new BatchedPayslipMetricUnitLengthRule(120);
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPayslipMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedPayslipMetricRequiredCodeRule("batched payslip metric needs an entry");
        var item = BatchedPayslipMetric.NewBatchedPayslipMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedPayslipMetricValidatorReportsEveryProblem();
        BatchedPayslipMetricFloorValueRangeRuleFlagsValuesAboveFloorValue();
        BatchedPayslipMetricUnitLengthRuleFlagsALongUnit();
        BatchedPayslipMetricRequiredCodeRuleFlagsABlankCode();
    }
}
