namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class LenientBenchmarkAuditChecks
{
    public static void LenientBenchmarkAuditStartsInCreated()
    {
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("code-716", "title-308");
        Check.Equal(item.Status, LenientBenchmarkAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBenchmarkAuditMovesToValidated()
    {
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("code-716", "title-308");
        try
        {
            item.MoveTo(LenientBenchmarkAuditStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBenchmarkAuditStatus.Validated, "status");
    }

    public static void LenientBenchmarkAuditRejectsMovesFromRetired()
    {
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("code-716", "title-308");
        item.Status = LenientBenchmarkAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientBenchmarkAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientBenchmarkAuditStartsInCreated();
        LenientBenchmarkAuditMovesToValidated();
        LenientBenchmarkAuditRejectsMovesFromRetired();
    }
}
