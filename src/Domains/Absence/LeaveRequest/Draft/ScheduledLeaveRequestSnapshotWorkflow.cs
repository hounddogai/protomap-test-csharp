namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled leave request snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledLeaveRequestSnapshotCoordinator(IScheduledLeaveRequestSnapshotStore Store, ScheduledLeaveRequestSnapshotValidator Validator, IClock Clock, List<string> Journal, ScheduledLeaveRequestSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledLeaveRequestSnapshotStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledLeaveRequestSnapshotStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request snapshot {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of scheduled leave request snapshot workflow.</summary>
public static class ScheduledLeaveRequestSnapshotWorkflow
{
    public static ScheduledLeaveRequestSnapshotCoordinator NewScheduledLeaveRequestSnapshotCoordinator(IScheduledLeaveRequestSnapshotStore store, IClock clock, Registry flags)
    {
        return new ScheduledLeaveRequestSnapshotCoordinator(
            store,
            ScheduledLeaveRequestSnapshotRules.DefaultScheduledLeaveRequestSnapshotValidator(),
            clock,
            new List<string>(),
            ScheduledLeaveRequestSnapshotEngine.DefaultScheduledLeaveRequestSnapshotEngine(flags));
    }
}
