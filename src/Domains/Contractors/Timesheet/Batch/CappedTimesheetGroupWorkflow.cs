namespace Acme.Workforce.Contractors.Timesheet.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped timesheet groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedTimesheetGroupCoordinator(ICappedTimesheetGroupStore Store, CappedTimesheetGroupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped timesheet group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedTimesheetGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped timesheet group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedTimesheetGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of capped timesheet group workflow.</summary>
public static class CappedTimesheetGroupWorkflow
{
    public static CappedTimesheetGroupCoordinator NewCappedTimesheetGroupCoordinator(ICappedTimesheetGroupStore store, IClock clock)
    {
        return new CappedTimesheetGroupCoordinator(
            store,
            CappedTimesheetGroupRules.DefaultCappedTimesheetGroupValidator(),
            clock,
            new List<string>());
    }
}
