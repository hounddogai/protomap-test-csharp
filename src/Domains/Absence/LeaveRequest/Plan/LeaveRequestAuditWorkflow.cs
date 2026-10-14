namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestAuditCoordinator(ILeaveRequestAuditStore Store, LeaveRequestAuditValidator Validator, IClock Clock, List<string> Journal, LeaveRequestAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestAuditStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestAuditStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request audit {code} not found");
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

/// <summary>Functions of leave request audit workflow.</summary>
public static class LeaveRequestAuditWorkflow
{
    public static LeaveRequestAuditCoordinator NewLeaveRequestAuditCoordinator(ILeaveRequestAuditStore store, IClock clock)
    {
        return new LeaveRequestAuditCoordinator(
            store,
            LeaveRequestAuditRules.DefaultLeaveRequestAuditValidator(),
            clock,
            new List<string>(),
            LeaveRequestAuditEngine.DefaultLeaveRequestAuditEngine());
    }
}
