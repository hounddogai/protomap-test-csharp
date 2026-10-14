namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves timesheet forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetForecastManager(ITimesheetForecastStore Store, TimesheetForecastValidator Validator, IClock Clock, List<string> Journal, TimesheetForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetForecastStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetForecastStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet forecast {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of timesheet forecast workflow.</summary>
public static class TimesheetForecastWorkflow
{
    public static TimesheetForecastManager NewTimesheetForecastManager(ITimesheetForecastStore store, IClock clock, Registry flags)
    {
        return new TimesheetForecastManager(
            store,
            TimesheetForecastRules.DefaultTimesheetForecastValidator(),
            clock,
            new List<string>(),
            TimesheetForecastEngine.DefaultTimesheetForecastEngine(flags));
    }
}
