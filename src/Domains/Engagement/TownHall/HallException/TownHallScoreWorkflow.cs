namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallScoreManager(ITownHallScoreStore Store, TownHallScoreValidator Validator, IClock Clock, List<string> Journal, TownHallScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall score {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of town hall score workflow.</summary>
public static class TownHallScoreWorkflow
{
    public static TownHallScoreManager NewTownHallScoreManager(ITownHallScoreStore store, IClock clock)
    {
        return new TownHallScoreManager(
            store,
            TownHallScoreRules.DefaultTownHallScoreValidator(),
            clock,
            new List<string>(),
            TownHallScoreEngine.DefaultTownHallScoreEngine());
    }
}
