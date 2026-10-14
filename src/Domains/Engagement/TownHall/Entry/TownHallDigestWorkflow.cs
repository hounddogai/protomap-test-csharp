namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallDigestCoordinator(ITownHallDigestStore Store, TownHallDigestValidator Validator, IClock Clock, List<string> Journal, TownHallDigestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallDigestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallDigestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall digest {code} not found");
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

/// <summary>Functions of town hall digest workflow.</summary>
public static class TownHallDigestWorkflow
{
    public static TownHallDigestCoordinator NewTownHallDigestCoordinator(ITownHallDigestStore store, IClock clock, Registry flags)
    {
        return new TownHallDigestCoordinator(
            store,
            TownHallDigestRules.DefaultTownHallDigestValidator(),
            clock,
            new List<string>(),
            TownHallDigestEngine.DefaultTownHallDigestEngine(flags));
    }
}
