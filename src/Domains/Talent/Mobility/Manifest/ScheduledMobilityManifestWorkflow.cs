namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled mobility manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledMobilityManifestDesk(IScheduledMobilityManifestStore Store, ScheduledMobilityManifestValidator Validator, IClock Clock, List<string> Journal, ScheduledMobilityManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledMobilityManifestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledMobilityManifestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility manifest {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of scheduled mobility manifest workflow.</summary>
public static class ScheduledMobilityManifestWorkflow
{
    public static ScheduledMobilityManifestDesk NewScheduledMobilityManifestDesk(IScheduledMobilityManifestStore store, IClock clock)
    {
        return new ScheduledMobilityManifestDesk(
            store,
            ScheduledMobilityManifestRules.DefaultScheduledMobilityManifestValidator(),
            clock,
            new List<string>(),
            ScheduledMobilityManifestEngine.DefaultScheduledMobilityManifestEngine());
    }
}
