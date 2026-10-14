namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkAuditChecks
{
    public static void BenchmarkAuditStartsInNew()
    {
        var item = BenchmarkAudit.NewBenchmarkAudit("code-974", "name-359");
        Check.Equal(item.Status, BenchmarkAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkAuditMovesToRunning()
    {
        var item = BenchmarkAudit.NewBenchmarkAudit("code-974", "name-359");
        try
        {
            item.MoveTo(BenchmarkAuditStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkAuditStatus.Running, "status");
    }

    public static void BenchmarkAuditRejectsMovesFromFailed()
    {
        var item = BenchmarkAudit.NewBenchmarkAudit("code-974", "name-359");
        item.Status = BenchmarkAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(BenchmarkAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        BenchmarkAuditStartsInNew();
        BenchmarkAuditMovesToRunning();
        BenchmarkAuditRejectsMovesFromFailed();
    }
}
