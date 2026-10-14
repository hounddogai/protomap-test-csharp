namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityMappingCoordinator(IMobilityMappingStore Store, MobilityMappingValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of mobility mapping workflow.</summary>
public static class MobilityMappingWorkflow
{
    public static MobilityMappingCoordinator NewMobilityMappingCoordinator(IMobilityMappingStore store, IClock clock)
    {
        return new MobilityMappingCoordinator(
            store,
            MobilityMappingRules.DefaultMobilityMappingValidator(),
            clock,
            new List<string>());
    }
}
