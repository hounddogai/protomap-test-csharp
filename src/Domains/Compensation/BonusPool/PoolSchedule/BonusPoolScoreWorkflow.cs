namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolScoreCoordinator(IBonusPoolScoreStore Store, BonusPoolScoreValidator Validator, IClock Clock, List<string> Journal, BonusPoolScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolScoreStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolScoreStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool score {code} not found");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of bonus pool score workflow.</summary>
public static class BonusPoolScoreWorkflow
{
    public static BonusPoolScoreCoordinator NewBonusPoolScoreCoordinator(IBonusPoolScoreStore store, IClock clock)
    {
        return new BonusPoolScoreCoordinator(
            store,
            BonusPoolScoreRules.DefaultBonusPoolScoreValidator(),
            clock,
            new List<string>(),
            BonusPoolScoreEngine.DefaultBonusPoolScoreEngine());
    }
}
