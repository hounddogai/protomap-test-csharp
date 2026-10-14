namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered merit cycle checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredMeritCycleCheckpointService(ITieredMeritCycleCheckpointStore Store, TieredMeritCycleCheckpointValidator Validator, IClock Clock, List<string> Journal, TieredMeritCycleCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered merit cycle checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredMeritCycleCheckpointStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered merit cycle checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredMeritCycleCheckpointStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered merit cycle checkpoint {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of tiered merit cycle checkpoint workflow.</summary>
public static class TieredMeritCycleCheckpointWorkflow
{
    public static TieredMeritCycleCheckpointService NewTieredMeritCycleCheckpointService(ITieredMeritCycleCheckpointStore store, IClock clock)
    {
        return new TieredMeritCycleCheckpointService(
            store,
            TieredMeritCycleCheckpointRules.DefaultTieredMeritCycleCheckpointValidator(),
            clock,
            new List<string>(),
            TieredMeritCycleCheckpointEngine.DefaultTieredMeritCycleCheckpointEngine());
    }
}
