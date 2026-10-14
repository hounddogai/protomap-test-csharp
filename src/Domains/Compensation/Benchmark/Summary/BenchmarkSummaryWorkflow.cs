namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves benchmark summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkSummaryDesk(IBenchmarkSummaryStore Store, BenchmarkSummaryValidator Validator, IClock Clock, List<string> Journal, BenchmarkSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkSummaryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkSummaryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark summary {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of benchmark summary workflow.</summary>
public static class BenchmarkSummaryWorkflow
{
    public static BenchmarkSummaryDesk NewBenchmarkSummaryDesk(IBenchmarkSummaryStore store, IClock clock, Registry flags)
    {
        return new BenchmarkSummaryDesk(
            store,
            BenchmarkSummaryRules.DefaultBenchmarkSummaryValidator(),
            clock,
            new List<string>(),
            BenchmarkSummaryEngine.DefaultBenchmarkSummaryEngine(flags));
    }
}
