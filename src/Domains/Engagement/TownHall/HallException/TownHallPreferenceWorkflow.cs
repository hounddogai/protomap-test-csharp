namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallPreferenceManager(ITownHallPreferenceStore Store, TownHallPreferenceValidator Validator, IClock Clock, List<string> Journal, TownHallPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallPreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallPreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall preference {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of town hall preference workflow.</summary>
public static class TownHallPreferenceWorkflow
{
    public static TownHallPreferenceManager NewTownHallPreferenceManager(ITownHallPreferenceStore store, IClock clock)
    {
        return new TownHallPreferenceManager(
            store,
            TownHallPreferenceRules.DefaultTownHallPreferenceValidator(),
            clock,
            new List<string>(),
            TownHallPreferenceEngine.DefaultTownHallPreferenceEngine());
    }
}
