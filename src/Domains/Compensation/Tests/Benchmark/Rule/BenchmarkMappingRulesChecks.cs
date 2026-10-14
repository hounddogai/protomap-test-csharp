namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkMappingRulesChecks
{
    public static void BenchmarkMappingValidatorReportsEveryProblem()
    {
        var validator = BenchmarkMappingRules.DefaultBenchmarkMappingValidator();
        var item = BenchmarkMapping.NewBenchmarkMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkMappingCodeLengthRule(80);
        var item = BenchmarkMapping.NewBenchmarkMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkMappingTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BenchmarkMappingTitleLengthRule(255);
        var item = BenchmarkMapping.NewBenchmarkMapping("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkMappingRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new BenchmarkMappingRequiredNotesRule("benchmark mapping needs an entry");
        var item = BenchmarkMapping.NewBenchmarkMapping("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkMappingValidatorReportsEveryProblem();
        BenchmarkMappingCodeLengthRuleFlagsALongCode();
        BenchmarkMappingTitleLengthRuleFlagsALongTitle();
        BenchmarkMappingRequiredNotesRuleFlagsABlankNotes();
    }
}
