namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class StrictBenchmarkAuditChecks
{
    public static void StrictBenchmarkAuditStartsInDraft()
    {
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("code-943", "label-720");
        Check.Equal(item.Status, StrictBenchmarkAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictBenchmarkAuditMovesToPending()
    {
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("code-943", "label-720");
        try
        {
            item.MoveTo(StrictBenchmarkAuditStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictBenchmarkAuditStatus.Pending, "status");
    }

    public static void StrictBenchmarkAuditRejectsMovesFromClosed()
    {
        var item = StrictBenchmarkAudit.NewStrictBenchmarkAudit("code-943", "label-720");
        item.Status = StrictBenchmarkAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictBenchmarkAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictBenchmarkAuditStartsInDraft();
        StrictBenchmarkAuditMovesToPending();
        StrictBenchmarkAuditRejectsMovesFromClosed();
    }
}
