namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped timesheet plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedTimesheetPlanDesk(ICappedTimesheetPlanStore Store, CappedTimesheetPlanValidator Validator, IClock Clock, List<string> Journal, CappedTimesheetPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped timesheet plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedTimesheetPlanStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped timesheet plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedTimesheetPlanStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped timesheet plan {code} not found");
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

/// <summary>Functions of capped timesheet plan workflow.</summary>
public static class CappedTimesheetPlanWorkflow
{
    public static CappedTimesheetPlanDesk NewCappedTimesheetPlanDesk(ICappedTimesheetPlanStore store, IClock clock, Registry flags)
    {
        return new CappedTimesheetPlanDesk(
            store,
            CappedTimesheetPlanRules.DefaultCappedTimesheetPlanValidator(),
            clock,
            new List<string>(),
            CappedTimesheetPlanEngine.DefaultCappedTimesheetPlanEngine(flags));
    }
}
