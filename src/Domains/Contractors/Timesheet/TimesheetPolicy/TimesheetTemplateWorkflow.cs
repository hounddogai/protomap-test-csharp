namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet templates through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetTemplateWorkflow(ITimesheetTemplateStore Store, TimesheetTemplateValidator Validator, IClock Clock, List<string> Journal)
{
    public static TimesheetTemplateWorkflow NewTimesheetTemplateWorkflow(ITimesheetTemplateStore store, IClock clock)
    {
        return new TimesheetTemplateWorkflow(
            store,
            TimesheetTemplateRules.DefaultTimesheetTemplateValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetTemplateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetTemplateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
