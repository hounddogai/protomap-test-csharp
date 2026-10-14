namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallTargetManager(ITownHallTargetStore Store, TownHallTargetValidator Validator, IClock Clock, List<string> Journal, TownHallTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallTargetStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallTargetStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall target {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of town hall target workflow.</summary>
public static class TownHallTargetWorkflow
{
    public static TownHallTargetManager NewTownHallTargetManager(ITownHallTargetStore store, IClock clock, Registry flags)
    {
        return new TownHallTargetManager(
            store,
            TownHallTargetRules.DefaultTownHallTargetValidator(),
            clock,
            new List<string>(),
            TownHallTargetEngine.DefaultTownHallTargetEngine(flags));
    }
}
