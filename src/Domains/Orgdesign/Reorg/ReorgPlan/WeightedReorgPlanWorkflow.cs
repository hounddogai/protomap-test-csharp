namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted reorg plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedReorgPlanCoordinator(IWeightedReorgPlanStore Store, WeightedReorgPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted reorg plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedReorgPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted reorg plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedReorgPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of weighted reorg plan workflow.</summary>
public static class WeightedReorgPlanWorkflow
{
    public static WeightedReorgPlanCoordinator NewWeightedReorgPlanCoordinator(IWeightedReorgPlanStore store, IClock clock)
    {
        return new WeightedReorgPlanCoordinator(
            store,
            WeightedReorgPlanRules.DefaultWeightedReorgPlanValidator(),
            clock,
            new List<string>());
    }
}
