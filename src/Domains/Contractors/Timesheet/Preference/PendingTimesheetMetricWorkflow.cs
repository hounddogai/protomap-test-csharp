namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending timesheet metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingTimesheetMetricManager(IPendingTimesheetMetricStore Store, PendingTimesheetMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending timesheet metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingTimesheetMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending timesheet metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingTimesheetMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of pending timesheet metric workflow.</summary>
public static class PendingTimesheetMetricWorkflow
{
    public static PendingTimesheetMetricManager NewPendingTimesheetMetricManager(IPendingTimesheetMetricStore store, IClock clock)
    {
        return new PendingTimesheetMetricManager(
            store,
            PendingTimesheetMetricRules.DefaultPendingTimesheetMetricValidator(),
            clock,
            new List<string>());
    }
}
