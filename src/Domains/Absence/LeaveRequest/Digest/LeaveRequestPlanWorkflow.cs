namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestPlanManager(ILeaveRequestPlanStore Store, LeaveRequestPlanValidator Validator, IClock Clock, List<string> Journal, LeaveRequestPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request plan {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of leave request plan workflow.</summary>
public static class LeaveRequestPlanWorkflow
{
    public static LeaveRequestPlanManager NewLeaveRequestPlanManager(ILeaveRequestPlanStore store, IClock clock)
    {
        return new LeaveRequestPlanManager(
            store,
            LeaveRequestPlanRules.DefaultLeaveRequestPlanValidator(),
            clock,
            new List<string>(),
            LeaveRequestPlanEngine.DefaultLeaveRequestPlanEngine());
    }
}
