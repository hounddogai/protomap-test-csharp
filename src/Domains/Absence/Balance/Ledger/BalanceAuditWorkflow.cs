namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves balance audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record BalanceAuditCoordinator(IBalanceAuditStore Store, BalanceAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BalanceAuditStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"balance audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BalanceAuditStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of balance audit workflow.</summary>
public static class BalanceAuditWorkflow
{
    public static BalanceAuditCoordinator NewBalanceAuditCoordinator(IBalanceAuditStore store, IClock clock)
    {
        return new BalanceAuditCoordinator(
            store,
            BalanceAuditRules.DefaultBalanceAuditValidator(),
            clock,
            new List<string>());
    }
}
