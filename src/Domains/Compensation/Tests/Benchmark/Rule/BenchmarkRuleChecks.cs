namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkRuleChecks
{
    public static void BenchmarkRuleStartsInCreated()
    {
        var item = BenchmarkRule.NewBenchmarkRule("code-174", "label-946");
        Check.Equal(item.Status, BenchmarkRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkRuleMovesToValidated()
    {
        var item = BenchmarkRule.NewBenchmarkRule("code-174", "label-946");
        try
        {
            item.MoveTo(BenchmarkRuleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkRuleStatus.Validated, "status");
    }

    public static void BenchmarkRuleRejectsMovesFromRetired()
    {
        var item = BenchmarkRule.NewBenchmarkRule("code-174", "label-946");
        item.Status = BenchmarkRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(BenchmarkRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        BenchmarkRuleStartsInCreated();
        BenchmarkRuleMovesToValidated();
        BenchmarkRuleRejectsMovesFromRetired();
    }
}
