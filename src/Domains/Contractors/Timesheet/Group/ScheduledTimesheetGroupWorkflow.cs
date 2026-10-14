namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled timesheet groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledTimesheetGroupDesk(IScheduledTimesheetGroupStore Store, ScheduledTimesheetGroupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled timesheet group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledTimesheetGroupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled timesheet group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledTimesheetGroupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of scheduled timesheet group workflow.</summary>
public static class ScheduledTimesheetGroupWorkflow
{
    public static ScheduledTimesheetGroupDesk NewScheduledTimesheetGroupDesk(IScheduledTimesheetGroupStore store, IClock clock)
    {
        return new ScheduledTimesheetGroupDesk(
            store,
            ScheduledTimesheetGroupRules.DefaultScheduledTimesheetGroupValidator(),
            clock,
            new List<string>());
    }
}
