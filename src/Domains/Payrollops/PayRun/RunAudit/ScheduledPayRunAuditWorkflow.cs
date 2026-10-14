namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pay run audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPayRunAuditDesk(IScheduledPayRunAuditStore Store, ScheduledPayRunAuditValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayRunAuditStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay run audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayRunAuditStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of scheduled pay run audit workflow.</summary>
public static class ScheduledPayRunAuditWorkflow
{
    public static ScheduledPayRunAuditDesk NewScheduledPayRunAuditDesk(IScheduledPayRunAuditStore store, IClock clock)
    {
        return new ScheduledPayRunAuditDesk(
            store,
            ScheduledPayRunAuditRules.DefaultScheduledPayRunAuditValidator(),
            clock,
            new List<string>());
    }
}
