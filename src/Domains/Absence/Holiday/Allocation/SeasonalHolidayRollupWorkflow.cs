namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal holiday rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalHolidayRollupService(ISeasonalHolidayRollupStore Store, SeasonalHolidayRollupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalHolidayRollupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal holiday rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalHolidayRollupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of seasonal holiday rollup workflow.</summary>
public static class SeasonalHolidayRollupWorkflow
{
    public static SeasonalHolidayRollupService NewSeasonalHolidayRollupService(ISeasonalHolidayRollupStore store, IClock clock)
    {
        return new SeasonalHolidayRollupService(
            store,
            SeasonalHolidayRollupRules.DefaultSeasonalHolidayRollupValidator(),
            clock,
            new List<string>());
    }
}
