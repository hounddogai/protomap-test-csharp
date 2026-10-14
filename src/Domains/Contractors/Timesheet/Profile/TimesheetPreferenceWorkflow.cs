namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetPreferenceManager(ITimesheetPreferenceStore Store, TimesheetPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetPreferenceStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetPreferenceStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of timesheet preference workflow.</summary>
public static class TimesheetPreferenceWorkflow
{
    public static TimesheetPreferenceManager NewTimesheetPreferenceManager(ITimesheetPreferenceStore store, IClock clock)
    {
        return new TimesheetPreferenceManager(
            store,
            TimesheetPreferenceRules.DefaultTimesheetPreferenceValidator(),
            clock,
            new List<string>());
    }
}
