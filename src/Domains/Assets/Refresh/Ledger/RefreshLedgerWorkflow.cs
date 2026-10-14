namespace Acme.Workforce.Assets.Refresh.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshLedgerDesk(IRefreshLedgerStore Store, RefreshLedgerValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshLedgerStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshLedgerStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of refresh ledger workflow.</summary>
public static class RefreshLedgerWorkflow
{
    public static RefreshLedgerDesk NewRefreshLedgerDesk(IRefreshLedgerStore store, IClock clock)
    {
        return new RefreshLedgerDesk(
            store,
            RefreshLedgerRules.DefaultRefreshLedgerValidator(),
            clock,
            new List<string>());
    }
}
