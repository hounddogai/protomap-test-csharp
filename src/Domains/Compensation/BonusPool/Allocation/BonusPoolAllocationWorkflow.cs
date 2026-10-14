namespace Acme.Workforce.Compensation.BonusPool.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool allocations through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolAllocationManager(IBonusPoolAllocationStore Store, BonusPoolAllocationValidator Validator, IClock Clock, List<string> Journal, BonusPoolAllocationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool allocation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolAllocationStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool allocation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolAllocationStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool allocation {code} not found");
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

/// <summary>Functions of bonus pool allocation workflow.</summary>
public static class BonusPoolAllocationWorkflow
{
    public static BonusPoolAllocationManager NewBonusPoolAllocationManager(IBonusPoolAllocationStore store, IClock clock, Registry flags)
    {
        return new BonusPoolAllocationManager(
            store,
            BonusPoolAllocationRules.DefaultBonusPoolAllocationValidator(),
            clock,
            new List<string>(),
            BonusPoolAllocationEngine.DefaultBonusPoolAllocationEngine(flags));
    }
}
