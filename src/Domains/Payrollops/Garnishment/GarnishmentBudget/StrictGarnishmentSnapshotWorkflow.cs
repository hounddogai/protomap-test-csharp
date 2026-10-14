namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict garnishment snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictGarnishmentSnapshotService(IStrictGarnishmentSnapshotStore Store, StrictGarnishmentSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict garnishment snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictGarnishmentSnapshotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict garnishment snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictGarnishmentSnapshotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of strict garnishment snapshot workflow.</summary>
public static class StrictGarnishmentSnapshotWorkflow
{
    public static StrictGarnishmentSnapshotService NewStrictGarnishmentSnapshotService(IStrictGarnishmentSnapshotStore store, IClock clock)
    {
        return new StrictGarnishmentSnapshotService(
            store,
            StrictGarnishmentSnapshotRules.DefaultStrictGarnishmentSnapshotValidator(),
            clock,
            new List<string>());
    }
}
