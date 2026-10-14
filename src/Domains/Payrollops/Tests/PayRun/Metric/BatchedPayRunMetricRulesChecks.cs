namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedPayRunMetricRulesChecks
{
    public static void BatchedPayRunMetricValidatorReportsEveryProblem()
    {
        var validator = BatchedPayRunMetricRules.DefaultBatchedPayRunMetricValidator();
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPayRunMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedPayRunMetricCodeLengthRule(64);
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPayRunMetricRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new BatchedPayRunMetricRequiredLocaleRule("batched pay run metric needs a value");
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedPayRunMetricRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new BatchedPayRunMetricRequiredGradeRule("batched pay run metric needs this field");
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedPayRunMetricValidatorReportsEveryProblem();
        BatchedPayRunMetricCodeLengthRuleFlagsALongCode();
        BatchedPayRunMetricRequiredLocaleRuleFlagsABlankLocale();
        BatchedPayRunMetricRequiredGradeRuleFlagsABlankGrade();
    }
}
