namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkTargetDesk(IBenchmarkTargetStore Store, BenchmarkTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkTargetStatus.Stopped);
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

/// <summary>Functions of benchmark target workflow.</summary>
public static class BenchmarkTargetWorkflow
{
    public static BenchmarkTargetDesk NewBenchmarkTargetDesk(IBenchmarkTargetStore store, IClock clock)
    {
        return new BenchmarkTargetDesk(
            store,
            BenchmarkTargetRules.DefaultBenchmarkTargetValidator(),
            clock,
            new List<string>());
    }
}
