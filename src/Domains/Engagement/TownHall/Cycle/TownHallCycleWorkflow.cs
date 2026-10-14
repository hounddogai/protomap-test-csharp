namespace Acme.Workforce.Engagement.TownHall.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallCycleManager(ITownHallCycleStore Store, TownHallCycleValidator Validator, IClock Clock, List<string> Journal, TownHallCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallCycleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallCycleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall cycle {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of town hall cycle workflow.</summary>
public static class TownHallCycleWorkflow
{
    public static TownHallCycleManager NewTownHallCycleManager(ITownHallCycleStore store, IClock clock)
    {
        return new TownHallCycleManager(
            store,
            TownHallCycleRules.DefaultTownHallCycleValidator(),
            clock,
            new List<string>(),
            TownHallCycleEngine.DefaultTownHallCycleEngine());
    }
}
