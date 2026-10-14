namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallBundleDesk(ITownHallBundleStore Store, TownHallBundleValidator Validator, IClock Clock, List<string> Journal, TownHallBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallBundleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallBundleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall bundle {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of town hall bundle workflow.</summary>
public static class TownHallBundleWorkflow
{
    public static TownHallBundleDesk NewTownHallBundleDesk(ITownHallBundleStore store, IClock clock, Registry flags)
    {
        return new TownHallBundleDesk(
            store,
            TownHallBundleRules.DefaultTownHallBundleValidator(),
            clock,
            new List<string>(),
            TownHallBundleEngine.DefaultTownHallBundleEngine(flags));
    }
}
