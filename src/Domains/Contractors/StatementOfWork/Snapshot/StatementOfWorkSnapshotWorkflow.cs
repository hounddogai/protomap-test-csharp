namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves statement of work snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StatementOfWorkSnapshotCoordinator(IStatementOfWorkSnapshotStore Store, StatementOfWorkSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StatementOfWorkSnapshotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"statement of work snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StatementOfWorkSnapshotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of statement of work snapshot workflow.</summary>
public static class StatementOfWorkSnapshotWorkflow
{
    public static StatementOfWorkSnapshotCoordinator NewStatementOfWorkSnapshotCoordinator(IStatementOfWorkSnapshotStore store, IClock clock)
    {
        return new StatementOfWorkSnapshotCoordinator(
            store,
            StatementOfWorkSnapshotRules.DefaultStatementOfWorkSnapshotValidator(),
            clock,
            new List<string>());
    }
}
