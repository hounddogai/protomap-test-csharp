namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestBaselineManager(ILeaveRequestBaselineStore Store, LeaveRequestBaselineValidator Validator, IClock Clock, List<string> Journal, LeaveRequestBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestBaselineStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestBaselineStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request baseline {code} not found");
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

/// <summary>Functions of leave request baseline workflow.</summary>
public static class LeaveRequestBaselineWorkflow
{
    public static LeaveRequestBaselineManager NewLeaveRequestBaselineManager(ILeaveRequestBaselineStore store, IClock clock)
    {
        return new LeaveRequestBaselineManager(
            store,
            LeaveRequestBaselineRules.DefaultLeaveRequestBaselineValidator(),
            clock,
            new List<string>(),
            LeaveRequestBaselineEngine.DefaultLeaveRequestBaselineEngine());
    }
}
