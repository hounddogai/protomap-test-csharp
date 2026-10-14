namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional leave request baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalLeaveRequestBaselineManager(IRegionalLeaveRequestBaselineStore Store, RegionalLeaveRequestBaselineValidator Validator, IClock Clock, List<string> Journal, RegionalLeaveRequestBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional leave request baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalLeaveRequestBaselineStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional leave request baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalLeaveRequestBaselineStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional leave request baseline {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of regional leave request baseline workflow.</summary>
public static class RegionalLeaveRequestBaselineWorkflow
{
    public static RegionalLeaveRequestBaselineManager NewRegionalLeaveRequestBaselineManager(IRegionalLeaveRequestBaselineStore store, IClock clock, Registry flags)
    {
        return new RegionalLeaveRequestBaselineManager(
            store,
            RegionalLeaveRequestBaselineRules.DefaultRegionalLeaveRequestBaselineValidator(),
            clock,
            new List<string>(),
            RegionalLeaveRequestBaselineEngine.DefaultRegionalLeaveRequestBaselineEngine(flags));
    }
}
