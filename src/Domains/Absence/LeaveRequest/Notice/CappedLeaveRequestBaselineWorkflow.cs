namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped leave request baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedLeaveRequestBaselineService(ICappedLeaveRequestBaselineStore Store, CappedLeaveRequestBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped leave request baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedLeaveRequestBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped leave request baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedLeaveRequestBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of capped leave request baseline workflow.</summary>
public static class CappedLeaveRequestBaselineWorkflow
{
    public static CappedLeaveRequestBaselineService NewCappedLeaveRequestBaselineService(ICappedLeaveRequestBaselineStore store, IClock clock)
    {
        return new CappedLeaveRequestBaselineService(
            store,
            CappedLeaveRequestBaselineRules.DefaultCappedLeaveRequestBaselineValidator(),
            clock,
            new List<string>());
    }
}
