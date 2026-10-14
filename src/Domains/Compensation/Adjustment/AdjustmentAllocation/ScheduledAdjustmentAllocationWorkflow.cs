namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled adjustment allocations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAdjustmentAllocationManager(IScheduledAdjustmentAllocationStore Store, ScheduledAdjustmentAllocationValidator Validator, IClock Clock, List<string> Journal, ScheduledAdjustmentAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAdjustmentAllocationStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAdjustmentAllocationStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled adjustment allocation {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of scheduled adjustment allocation workflow.</summary>
public static class ScheduledAdjustmentAllocationWorkflow
{
    public static ScheduledAdjustmentAllocationManager NewScheduledAdjustmentAllocationManager(IScheduledAdjustmentAllocationStore store, IClock clock)
    {
        return new ScheduledAdjustmentAllocationManager(
            store,
            ScheduledAdjustmentAllocationRules.DefaultScheduledAdjustmentAllocationValidator(),
            clock,
            new List<string>(),
            ScheduledAdjustmentAllocationEngine.DefaultScheduledAdjustmentAllocationEngine());
    }
}
