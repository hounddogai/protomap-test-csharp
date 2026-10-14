namespace Acme.Workforce.Absence.LeaveRequest.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves leave request adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestAdjustmentCoordinator(ILeaveRequestAdjustmentStore Store, LeaveRequestAdjustmentValidator Validator, IClock Clock, List<string> Journal, LeaveRequestAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestAdjustmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestAdjustmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request adjustment {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of leave request adjustment workflow.</summary>
public static class LeaveRequestAdjustmentWorkflow
{
    public static LeaveRequestAdjustmentCoordinator NewLeaveRequestAdjustmentCoordinator(ILeaveRequestAdjustmentStore store, IClock clock, Registry flags)
    {
        return new LeaveRequestAdjustmentCoordinator(
            store,
            LeaveRequestAdjustmentRules.DefaultLeaveRequestAdjustmentValidator(),
            clock,
            new List<string>(),
            LeaveRequestAdjustmentEngine.DefaultLeaveRequestAdjustmentEngine(flags));
    }
}
