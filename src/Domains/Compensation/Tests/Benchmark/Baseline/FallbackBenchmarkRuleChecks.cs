namespace Acme.Workforce.Compensation.Tests.Benchmark.Baseline;

using Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackBenchmarkRuleChecks
{
    public static void FallbackBenchmarkRuleStartsInProposed()
    {
        var item = FallbackBenchmarkRule.NewFallbackBenchmarkRule("code-516", "label-470");
        Check.Equal(item.Status, FallbackBenchmarkRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackBenchmarkRuleMovesToApproved()
    {
        var item = FallbackBenchmarkRule.NewFallbackBenchmarkRule("code-516", "label-470");
        try
        {
            item.MoveTo(FallbackBenchmarkRuleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackBenchmarkRuleStatus.Approved, "status");
    }

    public static void FallbackBenchmarkRuleRejectsMovesFromWithdrawn()
    {
        var item = FallbackBenchmarkRule.NewFallbackBenchmarkRule("code-516", "label-470");
        item.Status = FallbackBenchmarkRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackBenchmarkRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackBenchmarkRuleStartsInProposed();
        FallbackBenchmarkRuleMovesToApproved();
        FallbackBenchmarkRuleRejectsMovesFromWithdrawn();
    }
}
