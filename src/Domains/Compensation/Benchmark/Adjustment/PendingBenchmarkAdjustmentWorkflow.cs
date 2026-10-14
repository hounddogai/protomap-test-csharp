namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending benchmark adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingBenchmarkAdjustmentService(IPendingBenchmarkAdjustmentStore Store, PendingBenchmarkAdjustmentValidator Validator, IClock Clock, List<string> Journal, PendingBenchmarkAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending benchmark adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingBenchmarkAdjustmentStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending benchmark adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingBenchmarkAdjustmentStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending benchmark adjustment {code} not found");
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

/// <summary>Functions of pending benchmark adjustment workflow.</summary>
public static class PendingBenchmarkAdjustmentWorkflow
{
    public static PendingBenchmarkAdjustmentService NewPendingBenchmarkAdjustmentService(IPendingBenchmarkAdjustmentStore store, IClock clock)
    {
        return new PendingBenchmarkAdjustmentService(
            store,
            PendingBenchmarkAdjustmentRules.DefaultPendingBenchmarkAdjustmentValidator(),
            clock,
            new List<string>(),
            PendingBenchmarkAdjustmentEngine.DefaultPendingBenchmarkAdjustmentEngine());
    }
}
