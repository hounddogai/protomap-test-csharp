namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallBaselineCoordinator(ITownHallBaselineStore Store, TownHallBaselineValidator Validator, IClock Clock, List<string> Journal, TownHallBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallBaselineStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallBaselineStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall baseline {code} not found");
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

/// <summary>Functions of town hall baseline workflow.</summary>
public static class TownHallBaselineWorkflow
{
    public static TownHallBaselineCoordinator NewTownHallBaselineCoordinator(ITownHallBaselineStore store, IClock clock)
    {
        return new TownHallBaselineCoordinator(
            store,
            TownHallBaselineRules.DefaultTownHallBaselineValidator(),
            clock,
            new List<string>(),
            TownHallBaselineEngine.DefaultTownHallBaselineEngine());
    }
}
