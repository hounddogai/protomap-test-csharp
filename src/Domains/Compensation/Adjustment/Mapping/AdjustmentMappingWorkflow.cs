namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentMappingService(IAdjustmentMappingStore Store, AdjustmentMappingValidator Validator, IClock Clock, List<string> Journal, AdjustmentMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentMappingStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentMappingStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment mapping {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of adjustment mapping workflow.</summary>
public static class AdjustmentMappingWorkflow
{
    public static AdjustmentMappingService NewAdjustmentMappingService(IAdjustmentMappingStore store, IClock clock)
    {
        return new AdjustmentMappingService(
            store,
            AdjustmentMappingRules.DefaultAdjustmentMappingValidator(),
            clock,
            new List<string>(),
            AdjustmentMappingEngine.DefaultAdjustmentMappingEngine());
    }
}
