namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive merit cycle mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveMeritCycleMappingManager(IAdaptiveMeritCycleMappingStore Store, AdaptiveMeritCycleMappingValidator Validator, IClock Clock, List<string> Journal, AdaptiveMeritCycleMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive merit cycle mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveMeritCycleMappingStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive merit cycle mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveMeritCycleMappingStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive merit cycle mapping {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of adaptive merit cycle mapping workflow.</summary>
public static class AdaptiveMeritCycleMappingWorkflow
{
    public static AdaptiveMeritCycleMappingManager NewAdaptiveMeritCycleMappingManager(IAdaptiveMeritCycleMappingStore store, IClock clock)
    {
        return new AdaptiveMeritCycleMappingManager(
            store,
            AdaptiveMeritCycleMappingRules.DefaultAdaptiveMeritCycleMappingValidator(),
            clock,
            new List<string>(),
            AdaptiveMeritCycleMappingEngine.DefaultAdaptiveMeritCycleMappingEngine());
    }
}
