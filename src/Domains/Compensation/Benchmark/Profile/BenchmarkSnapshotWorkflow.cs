namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkSnapshotDesk(IBenchmarkSnapshotStore Store, BenchmarkSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkSnapshotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkSnapshotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of benchmark snapshot workflow.</summary>
public static class BenchmarkSnapshotWorkflow
{
    public static BenchmarkSnapshotDesk NewBenchmarkSnapshotDesk(IBenchmarkSnapshotStore store, IClock clock)
    {
        return new BenchmarkSnapshotDesk(
            store,
            BenchmarkSnapshotRules.DefaultBenchmarkSnapshotValidator(),
            clock,
            new List<string>());
    }
}
