namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled leave request baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledLeaveRequestBaselineCoordinator(IScheduledLeaveRequestBaselineStore Store, ScheduledLeaveRequestBaselineValidator Validator, IClock Clock, List<string> Journal, ScheduledLeaveRequestBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledLeaveRequestBaselineStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledLeaveRequestBaselineStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled leave request baseline {code} not found");
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

/// <summary>Functions of scheduled leave request baseline workflow.</summary>
public static class ScheduledLeaveRequestBaselineWorkflow
{
    public static ScheduledLeaveRequestBaselineCoordinator NewScheduledLeaveRequestBaselineCoordinator(IScheduledLeaveRequestBaselineStore store, IClock clock, Registry flags)
    {
        return new ScheduledLeaveRequestBaselineCoordinator(
            store,
            ScheduledLeaveRequestBaselineRules.DefaultScheduledLeaveRequestBaselineValidator(),
            clock,
            new List<string>(),
            ScheduledLeaveRequestBaselineEngine.DefaultScheduledLeaveRequestBaselineEngine(flags));
    }
}
