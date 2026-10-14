namespace Acme.Workforce.Compensation.Benchmark.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves benchmark entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkEntryService(IBenchmarkEntryStore Store, BenchmarkEntryValidator Validator, IClock Clock, List<string> Journal, BenchmarkEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark entry {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of benchmark entry workflow.</summary>
public static class BenchmarkEntryWorkflow
{
    public static BenchmarkEntryService NewBenchmarkEntryService(IBenchmarkEntryStore store, IClock clock, Registry flags)
    {
        return new BenchmarkEntryService(
            store,
            BenchmarkEntryRules.DefaultBenchmarkEntryValidator(),
            clock,
            new List<string>(),
            BenchmarkEntryEngine.DefaultBenchmarkEntryEngine(flags));
    }
}
