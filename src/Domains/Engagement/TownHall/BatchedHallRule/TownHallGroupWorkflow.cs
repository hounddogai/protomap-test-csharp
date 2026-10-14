namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallGroupManager(ITownHallGroupStore Store, TownHallGroupValidator Validator, IClock Clock, List<string> Journal, TownHallGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallGroupStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallGroupStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall group {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of town hall group workflow.</summary>
public static class TownHallGroupWorkflow
{
    public static TownHallGroupManager NewTownHallGroupManager(ITownHallGroupStore store, IClock clock, Registry flags)
    {
        return new TownHallGroupManager(
            store,
            TownHallGroupRules.DefaultTownHallGroupValidator(),
            clock,
            new List<string>(),
            TownHallGroupEngine.DefaultTownHallGroupEngine(flags));
    }
}
