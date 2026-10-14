namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive return plan thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveReturnPlanThresholdCoordinator(IAdaptiveReturnPlanThresholdStore Store, AdaptiveReturnPlanThresholdValidator Validator, IClock Clock, List<string> Journal, AdaptiveReturnPlanThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive return plan threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveReturnPlanThresholdStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive return plan threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveReturnPlanThresholdStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive return plan threshold {code} not found");
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

/// <summary>Functions of adaptive return plan threshold workflow.</summary>
public static class AdaptiveReturnPlanThresholdWorkflow
{
    public static AdaptiveReturnPlanThresholdCoordinator NewAdaptiveReturnPlanThresholdCoordinator(IAdaptiveReturnPlanThresholdStore store, IClock clock)
    {
        return new AdaptiveReturnPlanThresholdCoordinator(
            store,
            AdaptiveReturnPlanThresholdRules.DefaultAdaptiveReturnPlanThresholdValidator(),
            clock,
            new List<string>(),
            AdaptiveReturnPlanThresholdEngine.DefaultAdaptiveReturnPlanThresholdEngine());
    }
}
