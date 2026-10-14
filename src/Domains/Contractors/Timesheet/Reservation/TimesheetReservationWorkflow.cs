namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetReservationService(ITimesheetReservationStore Store, TimesheetReservationValidator Validator, IClock Clock, List<string> Journal, TimesheetReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetReservationStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetReservationStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet reservation {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of timesheet reservation workflow.</summary>
public static class TimesheetReservationWorkflow
{
    public static TimesheetReservationService NewTimesheetReservationService(ITimesheetReservationStore store, IClock clock)
    {
        return new TimesheetReservationService(
            store,
            TimesheetReservationRules.DefaultTimesheetReservationValidator(),
            clock,
            new List<string>(),
            TimesheetReservationEngine.DefaultTimesheetReservationEngine());
    }
}
