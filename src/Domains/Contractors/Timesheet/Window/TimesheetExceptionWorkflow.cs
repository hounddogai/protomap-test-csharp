namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetExceptionService(ITimesheetExceptionStore Store, TimesheetExceptionValidator Validator, IClock Clock, List<string> Journal, TimesheetExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetExceptionStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetExceptionStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet exception {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of timesheet exception workflow.</summary>
public static class TimesheetExceptionWorkflow
{
    public static TimesheetExceptionService NewTimesheetExceptionService(ITimesheetExceptionStore store, IClock clock)
    {
        return new TimesheetExceptionService(
            store,
            TimesheetExceptionRules.DefaultTimesheetExceptionValidator(),
            clock,
            new List<string>(),
            TimesheetExceptionEngine.DefaultTimesheetExceptionEngine());
    }
}
