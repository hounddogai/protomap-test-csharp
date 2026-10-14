namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves timesheet bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetBundleService(ITimesheetBundleStore Store, TimesheetBundleValidator Validator, IClock Clock, List<string> Journal, TimesheetBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetBundleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetBundleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet bundle {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of timesheet bundle workflow.</summary>
public static class TimesheetBundleWorkflow
{
    public static TimesheetBundleService NewTimesheetBundleService(ITimesheetBundleStore store, IClock clock, Registry flags)
    {
        return new TimesheetBundleService(
            store,
            TimesheetBundleRules.DefaultTimesheetBundleValidator(),
            clock,
            new List<string>(),
            TimesheetBundleEngine.DefaultTimesheetBundleEngine(flags));
    }
}
