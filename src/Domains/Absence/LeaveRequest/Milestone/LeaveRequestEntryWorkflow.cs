namespace Acme.Workforce.Absence.LeaveRequest.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestEntryCoordinator(ILeaveRequestEntryStore Store, LeaveRequestEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestEntryStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestEntryStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of leave request entry workflow.</summary>
public static class LeaveRequestEntryWorkflow
{
    public static LeaveRequestEntryCoordinator NewLeaveRequestEntryCoordinator(ILeaveRequestEntryStore store, IClock clock)
    {
        return new LeaveRequestEntryCoordinator(
            store,
            LeaveRequestEntryRules.DefaultLeaveRequestEntryValidator(),
            clock,
            new List<string>());
    }
}
