namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkMappingService(IBenchmarkMappingStore Store, BenchmarkMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkMappingStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkMappingStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of benchmark mapping workflow.</summary>
public static class BenchmarkMappingWorkflow
{
    public static BenchmarkMappingService NewBenchmarkMappingService(IBenchmarkMappingStore store, IClock clock)
    {
        return new BenchmarkMappingService(
            store,
            BenchmarkMappingRules.DefaultBenchmarkMappingValidator(),
            clock,
            new List<string>());
    }
}
