namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetPlanDesk(ITimesheetPlanStore Store, TimesheetPlanValidator Validator, IClock Clock, List<string> Journal, TimesheetPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetPlanStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetPlanStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet plan {code} not found");
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

/// <summary>Functions of timesheet plan workflow.</summary>
public static class TimesheetPlanWorkflow
{
    public static TimesheetPlanDesk NewTimesheetPlanDesk(ITimesheetPlanStore store, IClock clock)
    {
        return new TimesheetPlanDesk(
            store,
            TimesheetPlanRules.DefaultTimesheetPlanValidator(),
            clock,
            new List<string>(),
            TimesheetPlanEngine.DefaultTimesheetPlanEngine());
    }
}
