namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet records through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetRecordCoordinator(ITimesheetRecordStore Store, TimesheetRecordValidator Validator, IClock Clock, List<string> Journal, TimesheetRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetRecordStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetRecordStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet record {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of timesheet record workflow.</summary>
public static class TimesheetRecordWorkflow
{
    public static TimesheetRecordCoordinator NewTimesheetRecordCoordinator(ITimesheetRecordStore store, IClock clock)
    {
        return new TimesheetRecordCoordinator(
            store,
            TimesheetRecordRules.DefaultTimesheetRecordValidator(),
            clock,
            new List<string>(),
            TimesheetRecordEngine.DefaultTimesheetRecordEngine());
    }
}
