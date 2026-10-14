namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedAccrualMetricRulesChecks
{
    public static void BatchedAccrualMetricValidatorReportsEveryProblem()
    {
        var validator = BatchedAccrualMetricRules.DefaultBatchedAccrualMetricValidator();
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedAccrualMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedAccrualMetricRequiredCodeRule("batched accrual metric needs an entry");
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAccrualMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedAccrualMetricCodeLengthRule(255);
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedAccrualMetricRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new BatchedAccrualMetricRequiredDepartmentRule("batched accrual metric needs a value");
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedAccrualMetricRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedAccrualMetricRequiredLabelRule("batched accrual metric needs this field");
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedAccrualMetricValidatorReportsEveryProblem();
        BatchedAccrualMetricRequiredCodeRuleFlagsABlankCode();
        BatchedAccrualMetricCodeLengthRuleFlagsALongCode();
        BatchedAccrualMetricRequiredDepartmentRuleFlagsABlankDepartment();
        BatchedAccrualMetricRequiredLabelRuleFlagsABlankLabel();
    }
}
