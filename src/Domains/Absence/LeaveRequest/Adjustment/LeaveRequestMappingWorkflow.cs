namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves leave request mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestMappingDesk(ILeaveRequestMappingStore Store, LeaveRequestMappingValidator Validator, IClock Clock, List<string> Journal, LeaveRequestMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestMappingStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestMappingStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request mapping {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of leave request mapping workflow.</summary>
public static class LeaveRequestMappingWorkflow
{
    public static LeaveRequestMappingDesk NewLeaveRequestMappingDesk(ILeaveRequestMappingStore store, IClock clock, Registry flags)
    {
        return new LeaveRequestMappingDesk(
            store,
            LeaveRequestMappingRules.DefaultLeaveRequestMappingValidator(),
            clock,
            new List<string>(),
            LeaveRequestMappingEngine.DefaultLeaveRequestMappingEngine(flags));
    }
}
