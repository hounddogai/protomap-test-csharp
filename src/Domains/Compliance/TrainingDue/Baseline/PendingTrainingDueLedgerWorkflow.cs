namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending training due ledgers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingTrainingDueLedgerManager(IPendingTrainingDueLedgerStore Store, PendingTrainingDueLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending training due ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingTrainingDueLedgerStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending training due ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingTrainingDueLedgerStatus.Retired);
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

/// <summary>Functions of pending training due ledger workflow.</summary>
public static class PendingTrainingDueLedgerWorkflow
{
    public static PendingTrainingDueLedgerManager NewPendingTrainingDueLedgerManager(IPendingTrainingDueLedgerStore store, IClock clock)
    {
        return new PendingTrainingDueLedgerManager(
            store,
            PendingTrainingDueLedgerRules.DefaultPendingTrainingDueLedgerValidator(),
            clock,
            new List<string>());
    }
}
