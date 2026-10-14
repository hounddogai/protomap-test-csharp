namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetGroupService(ITimesheetGroupStore Store, TimesheetGroupValidator Validator, IClock Clock, List<string> Journal, TimesheetGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetGroupStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetGroupStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet group {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of timesheet group workflow.</summary>
public static class TimesheetGroupWorkflow
{
    public static TimesheetGroupService NewTimesheetGroupService(ITimesheetGroupStore store, IClock clock)
    {
        return new TimesheetGroupService(
            store,
            TimesheetGroupRules.DefaultTimesheetGroupValidator(),
            clock,
            new List<string>(),
            TimesheetGroupEngine.DefaultTimesheetGroupEngine());
    }
}
