namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves leave request windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestWindowService(ILeaveRequestWindowStore Store, LeaveRequestWindowValidator Validator, IClock Clock, List<string> Journal, LeaveRequestWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestWindowStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestWindowStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request window {code} not found");
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

/// <summary>Functions of leave request window workflow.</summary>
public static class LeaveRequestWindowWorkflow
{
    public static LeaveRequestWindowService NewLeaveRequestWindowService(ILeaveRequestWindowStore store, IClock clock, Registry flags)
    {
        return new LeaveRequestWindowService(
            store,
            LeaveRequestWindowRules.DefaultLeaveRequestWindowValidator(),
            clock,
            new List<string>(),
            LeaveRequestWindowEngine.DefaultLeaveRequestWindowEngine(flags));
    }
}
