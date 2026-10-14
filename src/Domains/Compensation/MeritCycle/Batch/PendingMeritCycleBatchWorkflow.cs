namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending merit cycle batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingMeritCycleBatchService(IPendingMeritCycleBatchStore Store, PendingMeritCycleBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending merit cycle batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingMeritCycleBatchStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending merit cycle batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingMeritCycleBatchStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of pending merit cycle batch workflow.</summary>
public static class PendingMeritCycleBatchWorkflow
{
    public static PendingMeritCycleBatchService NewPendingMeritCycleBatchService(IPendingMeritCycleBatchStore store, IClock clock)
    {
        return new PendingMeritCycleBatchService(
            store,
            PendingMeritCycleBatchRules.DefaultPendingMeritCycleBatchValidator(),
            clock,
            new List<string>());
    }
}
