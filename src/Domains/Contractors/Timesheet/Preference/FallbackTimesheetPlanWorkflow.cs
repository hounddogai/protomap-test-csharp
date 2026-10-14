namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback timesheet plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackTimesheetPlanDesk(IFallbackTimesheetPlanStore Store, FallbackTimesheetPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback timesheet plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackTimesheetPlanStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback timesheet plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackTimesheetPlanStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of fallback timesheet plan workflow.</summary>
public static class FallbackTimesheetPlanWorkflow
{
    public static FallbackTimesheetPlanDesk NewFallbackTimesheetPlanDesk(IFallbackTimesheetPlanStore store, IClock clock)
    {
        return new FallbackTimesheetPlanDesk(
            store,
            FallbackTimesheetPlanRules.DefaultFallbackTimesheetPlanValidator(),
            clock,
            new List<string>());
    }
}
