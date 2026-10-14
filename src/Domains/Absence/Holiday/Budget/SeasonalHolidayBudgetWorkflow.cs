namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal holiday budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalHolidayBudgetCoordinator(ISeasonalHolidayBudgetStore Store, SeasonalHolidayBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalHolidayBudgetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalHolidayBudgetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of seasonal holiday budget workflow.</summary>
public static class SeasonalHolidayBudgetWorkflow
{
    public static SeasonalHolidayBudgetCoordinator NewSeasonalHolidayBudgetCoordinator(ISeasonalHolidayBudgetStore store, IClock clock)
    {
        return new SeasonalHolidayBudgetCoordinator(
            store,
            SeasonalHolidayBudgetRules.DefaultSeasonalHolidayBudgetValidator(),
            clock,
            new List<string>());
    }
}
