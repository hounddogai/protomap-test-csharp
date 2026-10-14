namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolBatchService(IPoolBatchStore Store, PoolBatchValidator Validator, IClock Clock, List<string> Journal, PoolBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolBatchStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolBatchStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool batch {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of pool batch workflow.</summary>
public static class PoolBatchWorkflow
{
    public static PoolBatchService NewPoolBatchService(IPoolBatchStore store, IClock clock)
    {
        return new PoolBatchService(
            store,
            PoolBatchRules.DefaultPoolBatchValidator(),
            clock,
            new List<string>(),
            PoolBatchEngine.DefaultPoolBatchEngine());
    }
}
