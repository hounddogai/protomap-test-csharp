namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves timesheet segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetSegmentCoordinator(ITimesheetSegmentStore Store, TimesheetSegmentValidator Validator, IClock Clock, List<string> Journal, TimesheetSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetSegmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetSegmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet segment {code} not found");
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

/// <summary>Functions of timesheet segment workflow.</summary>
public static class TimesheetSegmentWorkflow
{
    public static TimesheetSegmentCoordinator NewTimesheetSegmentCoordinator(ITimesheetSegmentStore store, IClock clock, Registry flags)
    {
        return new TimesheetSegmentCoordinator(
            store,
            TimesheetSegmentRules.DefaultTimesheetSegmentValidator(),
            clock,
            new List<string>(),
            TimesheetSegmentEngine.DefaultTimesheetSegmentEngine(flags));
    }
}
