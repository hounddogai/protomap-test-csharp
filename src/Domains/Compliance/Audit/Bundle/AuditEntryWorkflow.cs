namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves audit entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditEntryManager(IAuditEntryStore Store, AuditEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditEntryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditEntryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of audit entry workflow.</summary>
public static class AuditEntryWorkflow
{
    public static AuditEntryManager NewAuditEntryManager(IAuditEntryStore store, IClock clock)
    {
        return new AuditEntryManager(store, AuditEntryRules.DefaultAuditEntryValidator(), clock, new List<string>());
    }
}
