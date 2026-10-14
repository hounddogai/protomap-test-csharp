namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves town hall manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallManifestService(ITownHallManifestStore Store, TownHallManifestValidator Validator, IClock Clock, List<string> Journal, TownHallManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallManifestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallManifestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall manifest {code} not found");
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

/// <summary>Functions of town hall manifest workflow.</summary>
public static class TownHallManifestWorkflow
{
    public static TownHallManifestService NewTownHallManifestService(ITownHallManifestStore store, IClock clock)
    {
        return new TownHallManifestService(
            store,
            TownHallManifestRules.DefaultTownHallManifestValidator(),
            clock,
            new List<string>(),
            TownHallManifestEngine.DefaultTownHallManifestEngine());
    }
}
