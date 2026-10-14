namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves timesheet thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record TimesheetThresholdManager(ITimesheetThresholdStore Store, TimesheetThresholdValidator Validator, IClock Clock, List<string> Journal, TimesheetThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TimesheetThresholdStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TimesheetThresholdStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"timesheet threshold {code} not found");
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

/// <summary>Functions of timesheet threshold workflow.</summary>
public static class TimesheetThresholdWorkflow
{
    public static TimesheetThresholdManager NewTimesheetThresholdManager(ITimesheetThresholdStore store, IClock clock)
    {
        return new TimesheetThresholdManager(
            store,
            TimesheetThresholdRules.DefaultTimesheetThresholdValidator(),
            clock,
            new List<string>(),
            TimesheetThresholdEngine.DefaultTimesheetThresholdEngine());
    }
}
