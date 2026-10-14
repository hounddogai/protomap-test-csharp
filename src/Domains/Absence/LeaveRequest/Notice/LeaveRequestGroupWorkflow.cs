namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves leave request groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestGroupManager(ILeaveRequestGroupStore Store, LeaveRequestGroupValidator Validator, IClock Clock, List<string> Journal, LeaveRequestGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request group {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of leave request group workflow.</summary>
public static class LeaveRequestGroupWorkflow
{
    public static LeaveRequestGroupManager NewLeaveRequestGroupManager(ILeaveRequestGroupStore store, IClock clock, Registry flags)
    {
        return new LeaveRequestGroupManager(
            store,
            LeaveRequestGroupRules.DefaultLeaveRequestGroupValidator(),
            clock,
            new List<string>(),
            LeaveRequestGroupEngine.DefaultLeaveRequestGroupEngine(flags));
    }
}
