namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary occupancy manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryOccupancyManifestManager(IPrimaryOccupancyManifestStore Store, PrimaryOccupancyManifestValidator Validator, IClock Clock, List<string> Journal, PrimaryOccupancyManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary occupancy manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryOccupancyManifestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary occupancy manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryOccupancyManifestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary occupancy manifest {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of primary occupancy manifest workflow.</summary>
public static class PrimaryOccupancyManifestWorkflow
{
    public static PrimaryOccupancyManifestManager NewPrimaryOccupancyManifestManager(IPrimaryOccupancyManifestStore store, IClock clock, Registry flags)
    {
        return new PrimaryOccupancyManifestManager(
            store,
            PrimaryOccupancyManifestRules.DefaultPrimaryOccupancyManifestValidator(),
            clock,
            new List<string>(),
            PrimaryOccupancyManifestEngine.DefaultPrimaryOccupancyManifestEngine(flags));
    }
}
