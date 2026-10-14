namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves audit drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditDraftService(IAuditDraftStore Store, AuditDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditDraftStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditDraftStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of audit draft workflow.</summary>
public static class AuditDraftWorkflow
{
    public static AuditDraftService NewAuditDraftService(IAuditDraftStore store, IClock clock)
    {
        return new AuditDraftService(store, AuditDraftRules.DefaultAuditDraftValidator(), clock, new List<string>());
    }
}
