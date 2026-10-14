namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallTierManager(ITownHallTierStore Store, TownHallTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallTierStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallTierStatus.Expired);
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

/// <summary>Functions of town hall tier workflow.</summary>
public static class TownHallTierWorkflow
{
    public static TownHallTierManager NewTownHallTierManager(ITownHallTierStore store, IClock clock)
    {
        return new TownHallTierManager(
            store,
            TownHallTierRules.DefaultTownHallTierValidator(),
            clock,
            new List<string>());
    }
}
