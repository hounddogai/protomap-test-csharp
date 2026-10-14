namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolEstimateCoordinator(IBonusPoolEstimateStore Store, BonusPoolEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolEstimateStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolEstimateStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of bonus pool estimate workflow.</summary>
public static class BonusPoolEstimateWorkflow
{
    public static BonusPoolEstimateCoordinator NewBonusPoolEstimateCoordinator(IBonusPoolEstimateStore store, IClock clock)
    {
        return new BonusPoolEstimateCoordinator(
            store,
            BonusPoolEstimateRules.DefaultBonusPoolEstimateValidator(),
            clock,
            new List<string>());
    }
}
