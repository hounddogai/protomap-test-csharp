namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetMetricWorkflow(ITimesheetMetricStore Store, TimesheetMetricValidator Validator, IClock Clock, List<string> Journal)
{
    public static TimesheetMetricWorkflow NewTimesheetMetricWorkflow(ITimesheetMetricStore store, IClock clock)
    {
        return new TimesheetMetricWorkflow(
            store,
            TimesheetMetricRules.DefaultTimesheetMetricValidator(),
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
            throw new InvalidOperationException($"timesheet metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetMetricStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetMetricStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
