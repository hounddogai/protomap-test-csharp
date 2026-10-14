namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched span allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedSpanAllocationCoordinator(IBatchedSpanAllocationStore Store, BatchedSpanAllocationValidator Validator, IClock Clock, List<string> Journal, BatchedSpanAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched span allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedSpanAllocationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched span allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedSpanAllocationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched span allocation {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of batched span allocation workflow.</summary>
public static class BatchedSpanAllocationWorkflow
{
    public static BatchedSpanAllocationCoordinator NewBatchedSpanAllocationCoordinator(IBatchedSpanAllocationStore store, IClock clock, Registry flags)
    {
        return new BatchedSpanAllocationCoordinator(
            store,
            BatchedSpanAllocationRules.DefaultBatchedSpanAllocationValidator(),
            clock,
            new List<string>(),
            BatchedSpanAllocationEngine.DefaultBatchedSpanAllocationEngine(flags));
    }
}
