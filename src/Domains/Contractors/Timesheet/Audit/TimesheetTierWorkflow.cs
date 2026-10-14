namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves timesheet tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetTierService(ITimesheetTierStore Store, TimesheetTierValidator Validator, IClock Clock, List<string> Journal, TimesheetTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetTierStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetTierStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet tier {code} not found");
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

/// <summary>Functions of timesheet tier workflow.</summary>
public static class TimesheetTierWorkflow
{
    public static TimesheetTierService NewTimesheetTierService(ITimesheetTierStore store, IClock clock, Registry flags)
    {
        return new TimesheetTierService(
            store,
            TimesheetTierRules.DefaultTimesheetTierValidator(),
            clock,
            new List<string>(),
            TimesheetTierEngine.DefaultTimesheetTierEngine(flags));
    }
}
