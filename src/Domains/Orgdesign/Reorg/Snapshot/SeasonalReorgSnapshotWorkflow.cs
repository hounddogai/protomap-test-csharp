namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal reorg snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalReorgSnapshotService(ISeasonalReorgSnapshotStore Store, SeasonalReorgSnapshotValidator Validator, IClock Clock, List<string> Journal, SeasonalReorgSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal reorg snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalReorgSnapshotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal reorg snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalReorgSnapshotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal reorg snapshot {code} not found");
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

/// <summary>Functions of seasonal reorg snapshot workflow.</summary>
public static class SeasonalReorgSnapshotWorkflow
{
    public static SeasonalReorgSnapshotService NewSeasonalReorgSnapshotService(ISeasonalReorgSnapshotStore store, IClock clock, Registry flags)
    {
        return new SeasonalReorgSnapshotService(
            store,
            SeasonalReorgSnapshotRules.DefaultSeasonalReorgSnapshotValidator(),
            clock,
            new List<string>(),
            SeasonalReorgSnapshotEngine.DefaultSeasonalReorgSnapshotEngine(flags));
    }
}
