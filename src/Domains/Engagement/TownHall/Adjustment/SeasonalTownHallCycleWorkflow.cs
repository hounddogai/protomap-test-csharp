namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal town hall cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalTownHallCycleCoordinator(ISeasonalTownHallCycleStore Store, SeasonalTownHallCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal town hall cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalTownHallCycleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal town hall cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalTownHallCycleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of seasonal town hall cycle workflow.</summary>
public static class SeasonalTownHallCycleWorkflow
{
    public static SeasonalTownHallCycleCoordinator NewSeasonalTownHallCycleCoordinator(ISeasonalTownHallCycleStore store, IClock clock)
    {
        return new SeasonalTownHallCycleCoordinator(
            store,
            SeasonalTownHallCycleRules.DefaultSeasonalTownHallCycleValidator(),
            clock,
            new List<string>());
    }
}
