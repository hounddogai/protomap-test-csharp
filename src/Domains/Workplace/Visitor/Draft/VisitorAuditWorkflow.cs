namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorAuditDesk(IVisitorAuditStore Store, VisitorAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorAuditStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorAuditStatus.Failed);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of visitor audit workflow.</summary>
public static class VisitorAuditWorkflow
{
    public static VisitorAuditDesk NewVisitorAuditDesk(IVisitorAuditStore store, IClock clock)
    {
        return new VisitorAuditDesk(
            store,
            VisitorAuditRules.DefaultVisitorAuditValidator(),
            clock,
            new List<string>());
    }
}
