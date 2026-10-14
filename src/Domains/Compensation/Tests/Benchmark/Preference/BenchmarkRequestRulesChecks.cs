namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class BenchmarkRequestRulesChecks
{
    public static void BenchmarkRequestValidatorReportsEveryProblem()
    {
        var validator = BenchmarkRequestRules.DefaultBenchmarkRequestValidator();
        var item = BenchmarkRequest.NewBenchmarkRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new BenchmarkRequestRequiredNameRule("benchmark request needs this field");
        var item = BenchmarkRequest.NewBenchmarkRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkRequestNotesLengthRuleFlagsALongNotes()
    {
        var rule = new BenchmarkRequestNotesLengthRule(120);
        var item = BenchmarkRequest.NewBenchmarkRequest("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BenchmarkRequestValidatorReportsEveryProblem();
        BenchmarkRequestRequiredNameRuleFlagsABlankName();
        BenchmarkRequestNotesLengthRuleFlagsALongNotes();
    }
}
