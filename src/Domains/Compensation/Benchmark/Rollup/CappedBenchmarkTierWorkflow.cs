namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped benchmark tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedBenchmarkTierCoordinator(ICappedBenchmarkTierStore Store, CappedBenchmarkTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped benchmark tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedBenchmarkTierStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped benchmark tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedBenchmarkTierStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of capped benchmark tier workflow.</summary>
public static class CappedBenchmarkTierWorkflow
{
    public static CappedBenchmarkTierCoordinator NewCappedBenchmarkTierCoordinator(ICappedBenchmarkTierStore store, IClock clock)
    {
        return new CappedBenchmarkTierCoordinator(
            store,
            CappedBenchmarkTierRules.DefaultCappedBenchmarkTierValidator(),
            clock,
            new List<string>());
    }
}
