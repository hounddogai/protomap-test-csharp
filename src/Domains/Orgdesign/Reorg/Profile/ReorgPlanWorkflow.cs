namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgPlanService(IReorgPlanStore Store, ReorgPlanValidator Validator, IClock Clock, List<string> Journal, ReorgPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgPlanStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgPlanStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg plan {code} not found");
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

/// <summary>Functions of reorg plan workflow.</summary>
public static class ReorgPlanWorkflow
{
    public static ReorgPlanService NewReorgPlanService(IReorgPlanStore store, IClock clock)
    {
        return new ReorgPlanService(
            store,
            ReorgPlanRules.DefaultReorgPlanValidator(),
            clock,
            new List<string>(),
            ReorgPlanEngine.DefaultReorgPlanEngine());
    }
}
