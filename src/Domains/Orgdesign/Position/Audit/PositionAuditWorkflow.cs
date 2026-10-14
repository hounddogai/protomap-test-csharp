namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves position audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionAuditDesk(IPositionAuditStore Store, PositionAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionAuditStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionAuditStatus.Closed);
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

/// <summary>Functions of position audit workflow.</summary>
public static class PositionAuditWorkflow
{
    public static PositionAuditDesk NewPositionAuditDesk(IPositionAuditStore store, IClock clock)
    {
        return new PositionAuditDesk(
            store,
            PositionAuditRules.DefaultPositionAuditValidator(),
            clock,
            new List<string>());
    }
}
