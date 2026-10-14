namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallEntryDesk(ITownHallEntryStore Store, TownHallEntryValidator Validator, IClock Clock, List<string> Journal, TownHallEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallEntryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallEntryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall entry {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of town hall entry workflow.</summary>
public static class TownHallEntryWorkflow
{
    public static TownHallEntryDesk NewTownHallEntryDesk(ITownHallEntryStore store, IClock clock, Registry flags)
    {
        return new TownHallEntryDesk(
            store,
            TownHallEntryRules.DefaultTownHallEntryValidator(),
            clock,
            new List<string>(),
            TownHallEntryEngine.DefaultTownHallEntryEngine(flags));
    }
}
