namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling training due ledgers through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingTrainingDueLedgerDesk(IRollingTrainingDueLedgerStore Store, RollingTrainingDueLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling training due ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingTrainingDueLedgerStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling training due ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingTrainingDueLedgerStatus.Failed);
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

/// <summary>Functions of rolling training due ledger workflow.</summary>
public static class RollingTrainingDueLedgerWorkflow
{
    public static RollingTrainingDueLedgerDesk NewRollingTrainingDueLedgerDesk(IRollingTrainingDueLedgerStore store, IClock clock)
    {
        return new RollingTrainingDueLedgerDesk(
            store,
            RollingTrainingDueLedgerRules.DefaultRollingTrainingDueLedgerValidator(),
            clock,
            new List<string>());
    }
}
