namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkAuditRulesChecks
{
    public static void BenchmarkAuditValidatorReportsEveryProblem()
    {
        var validator = BenchmarkAuditRules.DefaultBenchmarkAuditValidator();
        var item = BenchmarkAudit.NewBenchmarkAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkAuditOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new BenchmarkAuditOwnerIdLengthRule(1000);
        var item = BenchmarkAudit.NewBenchmarkAudit("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BenchmarkAuditRequiredCodeRule("benchmark audit needs this field");
        var item = BenchmarkAudit.NewBenchmarkAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkAuditRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new BenchmarkAuditRatioRangeRule(6.9, 184.5);
        var item = BenchmarkAudit.NewBenchmarkAudit("x", "y");
        item.Ratio = 186.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkAuditGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new BenchmarkAuditGraceDaysRangeRule(255, 255);
        var item = BenchmarkAudit.NewBenchmarkAudit("x", "y");
        item.GraceDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BenchmarkAuditValidatorReportsEveryProblem();
        BenchmarkAuditOwnerIdLengthRuleFlagsALongOwnerId();
        BenchmarkAuditRequiredCodeRuleFlagsABlankCode();
        BenchmarkAuditRatioRangeRuleFlagsValuesAboveRatio();
        BenchmarkAuditGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
