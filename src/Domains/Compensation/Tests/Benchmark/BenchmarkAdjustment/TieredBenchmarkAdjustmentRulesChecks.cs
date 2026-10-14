namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class TieredBenchmarkAdjustmentRulesChecks
{
    public static void TieredBenchmarkAdjustmentValidatorReportsEveryProblem()
    {
        var validator = TieredBenchmarkAdjustmentRules.DefaultTieredBenchmarkAdjustmentValidator();
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredBenchmarkAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new TieredBenchmarkAdjustmentNameLengthRule(32);
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredBenchmarkAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredBenchmarkAdjustmentCodeLengthRule(16);
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredBenchmarkAdjustmentNotesLengthRuleFlagsALongNotes()
    {
        var rule = new TieredBenchmarkAdjustmentNotesLengthRule(32);
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredBenchmarkAdjustmentValidatorReportsEveryProblem();
        TieredBenchmarkAdjustmentNameLengthRuleFlagsALongName();
        TieredBenchmarkAdjustmentCodeLengthRuleFlagsALongCode();
        TieredBenchmarkAdjustmentNotesLengthRuleFlagsALongNotes();
    }
}
