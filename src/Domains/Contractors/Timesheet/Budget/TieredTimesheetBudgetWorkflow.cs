namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered timesheet budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredTimesheetBudgetService(ITieredTimesheetBudgetStore Store, TieredTimesheetBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered timesheet budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredTimesheetBudgetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered timesheet budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredTimesheetBudgetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of tiered timesheet budget workflow.</summary>
public static class TieredTimesheetBudgetWorkflow
{
    public static TieredTimesheetBudgetService NewTieredTimesheetBudgetService(ITieredTimesheetBudgetStore store, IClock clock)
    {
        return new TieredTimesheetBudgetService(
            store,
            TieredTimesheetBudgetRules.DefaultTieredTimesheetBudgetValidator(),
            clock,
            new List<string>());
    }
}
