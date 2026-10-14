namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestScoreCoordinator(ILeaveRequestScoreStore Store, LeaveRequestScoreValidator Validator, IClock Clock, List<string> Journal, LeaveRequestScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestScoreStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestScoreStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request score {code} not found");
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

/// <summary>Functions of leave request score workflow.</summary>
public static class LeaveRequestScoreWorkflow
{
    public static LeaveRequestScoreCoordinator NewLeaveRequestScoreCoordinator(ILeaveRequestScoreStore store, IClock clock)
    {
        return new LeaveRequestScoreCoordinator(
            store,
            LeaveRequestScoreRules.DefaultLeaveRequestScoreValidator(),
            clock,
            new List<string>(),
            LeaveRequestScoreEngine.DefaultLeaveRequestScoreEngine());
    }
}
