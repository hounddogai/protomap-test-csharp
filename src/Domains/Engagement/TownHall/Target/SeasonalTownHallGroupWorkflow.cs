namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal town hall groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalTownHallGroupService(ISeasonalTownHallGroupStore Store, SeasonalTownHallGroupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal town hall group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalTownHallGroupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal town hall group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalTownHallGroupStatus.Failed);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of seasonal town hall group workflow.</summary>
public static class SeasonalTownHallGroupWorkflow
{
    public static SeasonalTownHallGroupService NewSeasonalTownHallGroupService(ISeasonalTownHallGroupStore store, IClock clock)
    {
        return new SeasonalTownHallGroupService(
            store,
            SeasonalTownHallGroupRules.DefaultSeasonalTownHallGroupValidator(),
            clock,
            new List<string>());
    }
}
