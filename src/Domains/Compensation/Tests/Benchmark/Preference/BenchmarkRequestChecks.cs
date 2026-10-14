namespace Acme.Workforce.Compensation.Tests.Benchmark.Preference;

using Acme.Workforce.Compensation.Benchmark.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class BenchmarkRequestChecks
{
    public static void BenchmarkRequestStartsInCreated()
    {
        var item = BenchmarkRequest.NewBenchmarkRequest("code-189", "name-374");
        Check.Equal(item.Status, BenchmarkRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BenchmarkRequestMovesToValidated()
    {
        var item = BenchmarkRequest.NewBenchmarkRequest("code-189", "name-374");
        try
        {
            item.MoveTo(BenchmarkRequestStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BenchmarkRequestStatus.Validated, "status");
    }

    public static void BenchmarkRequestRejectsMovesFromRetired()
    {
        var item = BenchmarkRequest.NewBenchmarkRequest("code-189", "name-374");
        item.Status = BenchmarkRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(BenchmarkRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        BenchmarkRequestStartsInCreated();
        BenchmarkRequestMovesToValidated();
        BenchmarkRequestRejectsMovesFromRetired();
    }
}
