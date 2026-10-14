namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestRuleManager(ILeaveRequestRuleStore Store, LeaveRequestRuleValidator Validator, IClock Clock, List<string> Journal, LeaveRequestRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestRuleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestRuleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request rule {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of leave request rule workflow.</summary>
public static class LeaveRequestRuleWorkflow
{
    public static LeaveRequestRuleManager NewLeaveRequestRuleManager(ILeaveRequestRuleStore store, IClock clock)
    {
        return new LeaveRequestRuleManager(
            store,
            LeaveRequestRuleRules.DefaultLeaveRequestRuleValidator(),
            clock,
            new List<string>(),
            LeaveRequestRuleEngine.DefaultLeaveRequestRuleEngine());
    }
}
