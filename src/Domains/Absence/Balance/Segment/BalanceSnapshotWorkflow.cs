namespace Acme.Workforce.Absence.Balance.Segment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceSnapshotManager(IBalanceSnapshotStore Store, BalanceSnapshotValidator Validator, IClock Clock, List<string> Journal, BalanceSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceSnapshotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceSnapshotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance snapshot {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of balance snapshot workflow.</summary>
public static class BalanceSnapshotWorkflow
{
    public static BalanceSnapshotManager NewBalanceSnapshotManager(IBalanceSnapshotStore store, IClock clock)
    {
        return new BalanceSnapshotManager(
            store,
            BalanceSnapshotRules.DefaultBalanceSnapshotValidator(),
            clock,
            new List<string>(),
            BalanceSnapshotEngine.DefaultBalanceSnapshotEngine());
    }
}
