namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetAssignmentWorkflow(ITimesheetAssignmentStore Store, TimesheetAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    public static TimesheetAssignmentWorkflow NewTimesheetAssignmentWorkflow(ITimesheetAssignmentStore store, IClock clock)
    {
        return new TimesheetAssignmentWorkflow(
            store,
            TimesheetAssignmentRules.DefaultTimesheetAssignmentValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetAssignmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
