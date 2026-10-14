namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive warranty mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveWarrantyMappingCoordinator(IAdaptiveWarrantyMappingStore Store, AdaptiveWarrantyMappingValidator Validator, IClock Clock, List<string> Journal, AdaptiveWarrantyMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive warranty mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveWarrantyMappingStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive warranty mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveWarrantyMappingStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive warranty mapping {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of adaptive warranty mapping workflow.</summary>
public static class AdaptiveWarrantyMappingWorkflow
{
    public static AdaptiveWarrantyMappingCoordinator NewAdaptiveWarrantyMappingCoordinator(IAdaptiveWarrantyMappingStore store, IClock clock)
    {
        return new AdaptiveWarrantyMappingCoordinator(
            store,
            AdaptiveWarrantyMappingRules.DefaultAdaptiveWarrantyMappingValidator(),
            clock,
            new List<string>(),
            AdaptiveWarrantyMappingEngine.DefaultAdaptiveWarrantyMappingEngine());
    }
}
