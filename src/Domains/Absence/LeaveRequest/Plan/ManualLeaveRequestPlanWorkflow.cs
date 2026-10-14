namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual leave request plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualLeaveRequestPlanService(IManualLeaveRequestPlanStore Store, ManualLeaveRequestPlanValidator Validator, IClock Clock, List<string> Journal, ManualLeaveRequestPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual leave request plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualLeaveRequestPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual leave request plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualLeaveRequestPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual leave request plan {code} not found");
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

/// <summary>Functions of manual leave request plan workflow.</summary>
public static class ManualLeaveRequestPlanWorkflow
{
    public static ManualLeaveRequestPlanService NewManualLeaveRequestPlanService(IManualLeaveRequestPlanStore store, IClock clock)
    {
        return new ManualLeaveRequestPlanService(
            store,
            ManualLeaveRequestPlanRules.DefaultManualLeaveRequestPlanValidator(),
            clock,
            new List<string>(),
            ManualLeaveRequestPlanEngine.DefaultManualLeaveRequestPlanEngine());
    }
}
