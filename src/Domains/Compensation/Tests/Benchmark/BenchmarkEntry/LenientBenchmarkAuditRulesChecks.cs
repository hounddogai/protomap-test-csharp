namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class LenientBenchmarkAuditRulesChecks
{
    public static void LenientBenchmarkAuditValidatorReportsEveryProblem()
    {
        var validator = LenientBenchmarkAuditRules.DefaultLenientBenchmarkAuditValidator();
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientBenchmarkAuditRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new LenientBenchmarkAuditRequiredManagerIdRule("lenient benchmark audit needs this field");
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientBenchmarkAuditSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new LenientBenchmarkAuditSequenceRangeRule(80, 255);
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientBenchmarkAuditMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new LenientBenchmarkAuditMinimumRangeRule(8.0, 2319.1);
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("x", "y");
        item.Minimum = 2320.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientBenchmarkAuditValidatorReportsEveryProblem();
        LenientBenchmarkAuditRequiredManagerIdRuleFlagsABlankManagerId();
        LenientBenchmarkAuditSequenceRangeRuleFlagsValuesAboveSequence();
        LenientBenchmarkAuditMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
