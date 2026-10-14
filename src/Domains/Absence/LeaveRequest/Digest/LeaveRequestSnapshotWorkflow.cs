namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves leave request snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestSnapshotCoordinator(ILeaveRequestSnapshotStore Store, LeaveRequestSnapshotValidator Validator, IClock Clock, List<string> Journal, LeaveRequestSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestSnapshotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestSnapshotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request snapshot {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of leave request snapshot workflow.</summary>
public static class LeaveRequestSnapshotWorkflow
{
    public static LeaveRequestSnapshotCoordinator NewLeaveRequestSnapshotCoordinator(ILeaveRequestSnapshotStore store, IClock clock, Registry flags)
    {
        return new LeaveRequestSnapshotCoordinator(
            store,
            LeaveRequestSnapshotRules.DefaultLeaveRequestSnapshotValidator(),
            clock,
            new List<string>(),
            LeaveRequestSnapshotEngine.DefaultLeaveRequestSnapshotEngine(flags));
    }
}
