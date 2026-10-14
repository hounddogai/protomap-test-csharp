namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling leave request adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingLeaveRequestAdjustmentManager(IRollingLeaveRequestAdjustmentStore Store, RollingLeaveRequestAdjustmentValidator Validator, IClock Clock, List<string> Journal, RollingLeaveRequestAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling leave request adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingLeaveRequestAdjustmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling leave request adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingLeaveRequestAdjustmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling leave request adjustment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of rolling leave request adjustment workflow.</summary>
public static class RollingLeaveRequestAdjustmentWorkflow
{
    public static RollingLeaveRequestAdjustmentManager NewRollingLeaveRequestAdjustmentManager(IRollingLeaveRequestAdjustmentStore store, IClock clock, Registry flags)
    {
        return new RollingLeaveRequestAdjustmentManager(
            store,
            RollingLeaveRequestAdjustmentRules.DefaultRollingLeaveRequestAdjustmentValidator(),
            clock,
            new List<string>(),
            RollingLeaveRequestAdjustmentEngine.DefaultRollingLeaveRequestAdjustmentEngine(flags));
    }
}
