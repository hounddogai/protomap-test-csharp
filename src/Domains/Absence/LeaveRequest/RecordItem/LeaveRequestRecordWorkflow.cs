namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves leave request records through their life cycle and keeps a journal of what it did.</summary>
public sealed record LeaveRequestRecordCoordinator(ILeaveRequestRecordStore Store, LeaveRequestRecordValidator Validator, IClock Clock, List<string> Journal, LeaveRequestRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LeaveRequestRecordStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LeaveRequestRecordStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"leave request record {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of leave request record workflow.</summary>
public static class LeaveRequestRecordWorkflow
{
    public static LeaveRequestRecordCoordinator NewLeaveRequestRecordCoordinator(ILeaveRequestRecordStore store, IClock clock)
    {
        return new LeaveRequestRecordCoordinator(
            store,
            LeaveRequestRecordRules.DefaultLeaveRequestRecordValidator(),
            clock,
            new List<string>(),
            LeaveRequestRecordEngine.DefaultLeaveRequestRecordEngine());
    }
}
