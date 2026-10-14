namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves benchmark thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record BenchmarkThresholdService(IBenchmarkThresholdStore Store, BenchmarkThresholdValidator Validator, IClock Clock, List<string> Journal, BenchmarkThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BenchmarkThresholdStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BenchmarkThresholdStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"benchmark threshold {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of benchmark threshold workflow.</summary>
public static class BenchmarkThresholdWorkflow
{
    public static BenchmarkThresholdService NewBenchmarkThresholdService(IBenchmarkThresholdStore store, IClock clock)
    {
        return new BenchmarkThresholdService(
            store,
            BenchmarkThresholdRules.DefaultBenchmarkThresholdValidator(),
            clock,
            new List<string>(),
            BenchmarkThresholdEngine.DefaultBenchmarkThresholdEngine());
    }
}
