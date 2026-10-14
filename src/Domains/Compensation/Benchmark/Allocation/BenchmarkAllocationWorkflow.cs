namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkAllocationCoordinator(IBenchmarkAllocationStore Store, BenchmarkAllocationValidator Validator, IClock Clock, List<string> Journal, BenchmarkAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkAllocationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkAllocationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark allocation {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of benchmark allocation workflow.</summary>
public static class BenchmarkAllocationWorkflow
{
    public static BenchmarkAllocationCoordinator NewBenchmarkAllocationCoordinator(IBenchmarkAllocationStore store, IClock clock)
    {
        return new BenchmarkAllocationCoordinator(
            store,
            BenchmarkAllocationRules.DefaultBenchmarkAllocationValidator(),
            clock,
            new List<string>(),
            BenchmarkAllocationEngine.DefaultBenchmarkAllocationEngine());
    }
}
