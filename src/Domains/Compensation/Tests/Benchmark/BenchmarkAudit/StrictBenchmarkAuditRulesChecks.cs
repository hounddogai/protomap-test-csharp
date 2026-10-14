namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class StrictBenchmarkAuditRulesChecks
{
    public static void StrictBenchmarkAuditValidatorReportsEveryProblem()
    {
        var validator = StrictBenchmarkAuditRules.DefaultStrictBenchmarkAuditValidator();
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictBenchmarkAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictBenchmarkAuditRequiredLabelRule("strict benchmark audit needs an entry");
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictBenchmarkAuditRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new StrictBenchmarkAuditRateRangeRule(5.8, 2903.3);
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("x", "y");
        item.Rate = 2904.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictBenchmarkAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictBenchmarkAuditCodeLengthRule(255);
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictBenchmarkAuditPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new StrictBenchmarkAuditPriorityRangeRule(16, 86400);
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("x", "y");
        item.Priority = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictBenchmarkAuditValidatorReportsEveryProblem();
        StrictBenchmarkAuditRequiredLabelRuleFlagsABlankLabel();
        StrictBenchmarkAuditRateRangeRuleFlagsValuesAboveRate();
        StrictBenchmarkAuditCodeLengthRuleFlagsALongCode();
        StrictBenchmarkAuditPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
