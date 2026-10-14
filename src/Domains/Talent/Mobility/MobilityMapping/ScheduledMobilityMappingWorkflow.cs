namespace Acme.Workforce.Talent.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled mobility mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledMobilityMappingManager(IScheduledMobilityMappingStore Store, ScheduledMobilityMappingValidator Validator, IClock Clock, List<string> Journal, ScheduledMobilityMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledMobilityMappingStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledMobilityMappingStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility mapping {code} not found");
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

/// <summary>Functions of scheduled mobility mapping workflow.</summary>
public static class ScheduledMobilityMappingWorkflow
{
    public static ScheduledMobilityMappingManager NewScheduledMobilityMappingManager(IScheduledMobilityMappingStore store, IClock clock)
    {
        return new ScheduledMobilityMappingManager(
            store,
            ScheduledMobilityMappingRules.DefaultScheduledMobilityMappingValidator(),
            clock,
            new List<string>(),
            ScheduledMobilityMappingEngine.DefaultScheduledMobilityMappingEngine());
    }
}
