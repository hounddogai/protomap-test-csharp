namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolSnapshotCoordinator(IBonusPoolSnapshotStore Store, BonusPoolSnapshotValidator Validator, IClock Clock, List<string> Journal, BonusPoolSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolSnapshotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolSnapshotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool snapshot {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of bonus pool snapshot workflow.</summary>
public static class BonusPoolSnapshotWorkflow
{
    public static BonusPoolSnapshotCoordinator NewBonusPoolSnapshotCoordinator(IBonusPoolSnapshotStore store, IClock clock, Registry flags)
    {
        return new BonusPoolSnapshotCoordinator(
            store,
            BonusPoolSnapshotRules.DefaultBonusPoolSnapshotValidator(),
            clock,
            new List<string>(),
            BonusPoolSnapshotEngine.DefaultBonusPoolSnapshotEngine(flags));
    }
}
