namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling benchmark summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingBenchmarkSummaryDesk(IRollingBenchmarkSummaryStore Store, RollingBenchmarkSummaryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling benchmark summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingBenchmarkSummaryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling benchmark summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingBenchmarkSummaryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of rolling benchmark summary workflow.</summary>
public static class RollingBenchmarkSummaryWorkflow
{
    public static RollingBenchmarkSummaryDesk NewRollingBenchmarkSummaryDesk(IRollingBenchmarkSummaryStore store, IClock clock)
    {
        return new RollingBenchmarkSummaryDesk(
            store,
            RollingBenchmarkSummaryRules.DefaultRollingBenchmarkSummaryValidator(),
            clock,
            new List<string>());
    }
}
