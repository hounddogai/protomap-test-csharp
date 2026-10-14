namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual benchmark allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualBenchmarkAllocationService(IManualBenchmarkAllocationStore Store, ManualBenchmarkAllocationValidator Validator, IClock Clock, List<string> Journal, ManualBenchmarkAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual benchmark allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualBenchmarkAllocationStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual benchmark allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualBenchmarkAllocationStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual benchmark allocation {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of manual benchmark allocation workflow.</summary>
public static class ManualBenchmarkAllocationWorkflow
{
    public static ManualBenchmarkAllocationService NewManualBenchmarkAllocationService(IManualBenchmarkAllocationStore store, IClock clock, Registry flags)
    {
        return new ManualBenchmarkAllocationService(
            store,
            ManualBenchmarkAllocationRules.DefaultManualBenchmarkAllocationValidator(),
            clock,
            new List<string>(),
            ManualBenchmarkAllocationEngine.DefaultManualBenchmarkAllocationEngine(flags));
    }
}
