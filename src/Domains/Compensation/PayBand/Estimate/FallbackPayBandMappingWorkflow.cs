namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback pay band mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackPayBandMappingDesk(IFallbackPayBandMappingStore Store, FallbackPayBandMappingValidator Validator, IClock Clock, List<string> Journal, FallbackPayBandMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pay band mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPayBandMappingStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pay band mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPayBandMappingStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pay band mapping {code} not found");
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

/// <summary>Functions of fallback pay band mapping workflow.</summary>
public static class FallbackPayBandMappingWorkflow
{
    public static FallbackPayBandMappingDesk NewFallbackPayBandMappingDesk(IFallbackPayBandMappingStore store, IClock clock)
    {
        return new FallbackPayBandMappingDesk(
            store,
            FallbackPayBandMappingRules.DefaultFallbackPayBandMappingValidator(),
            clock,
            new List<string>(),
            FallbackPayBandMappingEngine.DefaultFallbackPayBandMappingEngine());
    }
}
