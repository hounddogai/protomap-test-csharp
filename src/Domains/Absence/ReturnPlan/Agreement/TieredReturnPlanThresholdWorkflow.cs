namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered return plan thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredReturnPlanThresholdManager(ITieredReturnPlanThresholdStore Store, TieredReturnPlanThresholdValidator Validator, IClock Clock, List<string> Journal, TieredReturnPlanThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredReturnPlanThresholdStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredReturnPlanThresholdStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered return plan threshold {code} not found");
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

/// <summary>Functions of tiered return plan threshold workflow.</summary>
public static class TieredReturnPlanThresholdWorkflow
{
    public static TieredReturnPlanThresholdManager NewTieredReturnPlanThresholdManager(ITieredReturnPlanThresholdStore store, IClock clock)
    {
        return new TieredReturnPlanThresholdManager(
            store,
            TieredReturnPlanThresholdRules.DefaultTieredReturnPlanThresholdValidator(),
            clock,
            new List<string>(),
            TieredReturnPlanThresholdEngine.DefaultTieredReturnPlanThresholdEngine());
    }
}
