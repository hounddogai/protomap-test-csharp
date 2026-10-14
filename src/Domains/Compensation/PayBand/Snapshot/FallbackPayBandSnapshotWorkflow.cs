namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback pay band snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackPayBandSnapshotManager(IFallbackPayBandSnapshotStore Store, FallbackPayBandSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pay band snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPayBandSnapshotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pay band snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPayBandSnapshotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of fallback pay band snapshot workflow.</summary>
public static class FallbackPayBandSnapshotWorkflow
{
    public static FallbackPayBandSnapshotManager NewFallbackPayBandSnapshotManager(IFallbackPayBandSnapshotStore store, IClock clock)
    {
        return new FallbackPayBandSnapshotManager(
            store,
            FallbackPayBandSnapshotRules.DefaultFallbackPayBandSnapshotValidator(),
            clock,
            new List<string>());
    }
}
