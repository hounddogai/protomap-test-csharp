namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive action plan tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveActionPlanTierWorkflow(IAdaptiveActionPlanTierStore Store, AdaptiveActionPlanTierValidator Validator, IClock Clock, List<string> Journal, AdaptiveActionPlanTierEngine Engine)
{
    public static AdaptiveActionPlanTierWorkflow NewAdaptiveActionPlanTierWorkflow(IAdaptiveActionPlanTierStore store, IClock clock)
    {
        return new AdaptiveActionPlanTierWorkflow(
            store,
            AdaptiveActionPlanTierRules.DefaultAdaptiveActionPlanTierValidator(),
            clock,
            new List<string>(),
            AdaptiveActionPlanTierEngine.DefaultAdaptiveActionPlanTierEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive action plan tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveActionPlanTierStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive action plan tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveActionPlanTierStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive action plan tier {code} not found");
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
