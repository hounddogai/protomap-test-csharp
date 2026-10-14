namespace Acme.Workforce.Engagement.ActionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive action plan slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveActionPlanSlotManager(IAdaptiveActionPlanSlotStore Store, AdaptiveActionPlanSlotValidator Validator, IClock Clock, List<string> Journal, AdaptiveActionPlanSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive action plan slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveActionPlanSlotStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive action plan slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveActionPlanSlotStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive action plan slot {code} not found");
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

/// <summary>Functions of adaptive action plan slot workflow.</summary>
public static class AdaptiveActionPlanSlotWorkflow
{
    public static AdaptiveActionPlanSlotManager NewAdaptiveActionPlanSlotManager(IAdaptiveActionPlanSlotStore store, IClock clock, Registry flags)
    {
        return new AdaptiveActionPlanSlotManager(
            store,
            AdaptiveActionPlanSlotRules.DefaultAdaptiveActionPlanSlotValidator(),
            clock,
            new List<string>(),
            AdaptiveActionPlanSlotEngine.DefaultAdaptiveActionPlanSlotEngine(flags));
    }
}
