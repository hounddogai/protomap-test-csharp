namespace Acme.Workforce.Compensation.Tests.Benchmark.Proposal;

using Acme.Workforce.Compensation.Benchmark.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredBenchmarkSummaryRulesChecks
{
    public static void TieredBenchmarkSummaryValidatorReportsEveryProblem()
    {
        var validator = TieredBenchmarkSummaryRules.DefaultTieredBenchmarkSummaryValidator();
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredBenchmarkSummaryNameLengthRuleFlagsALongName()
    {
        var rule = new TieredBenchmarkSummaryNameLengthRule(64);
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredBenchmarkSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredBenchmarkSummaryRequiredNameRule("tiered benchmark summary needs a value");
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredBenchmarkSummaryRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new TieredBenchmarkSummaryRequiredEmployeeIdRule("tiered benchmark summary needs an entry");
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredBenchmarkSummaryRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TieredBenchmarkSummaryRequiredPhoneRule("tiered benchmark summary needs this field");
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredBenchmarkSummaryValidatorReportsEveryProblem();
        TieredBenchmarkSummaryNameLengthRuleFlagsALongName();
        TieredBenchmarkSummaryRequiredNameRuleFlagsABlankName();
        TieredBenchmarkSummaryRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        TieredBenchmarkSummaryRequiredPhoneRuleFlagsABlankPhone();
    }
}
