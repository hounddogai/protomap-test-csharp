namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict timesheet estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictTimesheetEstimateDesk(IStrictTimesheetEstimateStore Store, StrictTimesheetEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict timesheet estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictTimesheetEstimateStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict timesheet estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictTimesheetEstimateStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of strict timesheet estimate workflow.</summary>
public static class StrictTimesheetEstimateWorkflow
{
    public static StrictTimesheetEstimateDesk NewStrictTimesheetEstimateDesk(IStrictTimesheetEstimateStore store, IClock clock)
    {
        return new StrictTimesheetEstimateDesk(
            store,
            StrictTimesheetEstimateRules.DefaultStrictTimesheetEstimateValidator(),
            clock,
            new List<string>());
    }
}
