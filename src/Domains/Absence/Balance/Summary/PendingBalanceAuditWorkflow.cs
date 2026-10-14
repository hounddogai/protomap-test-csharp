namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending balance audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingBalanceAuditService(IPendingBalanceAuditStore Store, PendingBalanceAuditValidator Validator, IClock Clock, List<string> Journal, PendingBalanceAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending balance audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingBalanceAuditStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending balance audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingBalanceAuditStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending balance audit {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of pending balance audit workflow.</summary>
public static class PendingBalanceAuditWorkflow
{
    public static PendingBalanceAuditService NewPendingBalanceAuditService(IPendingBalanceAuditStore store, IClock clock, Registry flags)
    {
        return new PendingBalanceAuditService(
            store,
            PendingBalanceAuditRules.DefaultPendingBalanceAuditValidator(),
            clock,
            new List<string>(),
            PendingBalanceAuditEngine.DefaultPendingBalanceAuditEngine(flags));
    }
}
