namespace Acme.Workforce.Compensation.Benchmark.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual benchmark windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualBenchmarkWindowCoordinator(IManualBenchmarkWindowStore Store, ManualBenchmarkWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual benchmark window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualBenchmarkWindowStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual benchmark window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualBenchmarkWindowStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of manual benchmark window workflow.</summary>
public static class ManualBenchmarkWindowWorkflow
{
    public static ManualBenchmarkWindowCoordinator NewManualBenchmarkWindowCoordinator(IManualBenchmarkWindowStore store, IClock clock)
    {
        return new ManualBenchmarkWindowCoordinator(
            store,
            ManualBenchmarkWindowRules.DefaultManualBenchmarkWindowValidator(),
            clock,
            new List<string>());
    }
}
