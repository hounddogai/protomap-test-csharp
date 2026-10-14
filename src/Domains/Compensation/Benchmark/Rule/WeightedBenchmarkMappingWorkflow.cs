namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted benchmark mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedBenchmarkMappingService(IWeightedBenchmarkMappingStore Store, WeightedBenchmarkMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted benchmark mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedBenchmarkMappingStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted benchmark mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedBenchmarkMappingStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of weighted benchmark mapping workflow.</summary>
public static class WeightedBenchmarkMappingWorkflow
{
    public static WeightedBenchmarkMappingService NewWeightedBenchmarkMappingService(IWeightedBenchmarkMappingStore store, IClock clock)
    {
        return new WeightedBenchmarkMappingService(
            store,
            WeightedBenchmarkMappingRules.DefaultWeightedBenchmarkMappingValidator(),
            clock,
            new List<string>());
    }
}
