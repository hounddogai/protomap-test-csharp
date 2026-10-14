namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolOverrideManager(IBonusPoolOverrideStore Store, BonusPoolOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolOverrideStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolOverrideStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of bonus pool override workflow.</summary>
public static class BonusPoolOverrideWorkflow
{
    public static BonusPoolOverrideManager NewBonusPoolOverrideManager(IBonusPoolOverrideStore store, IClock clock)
    {
        return new BonusPoolOverrideManager(
            store,
            BonusPoolOverrideRules.DefaultBonusPoolOverrideValidator(),
            clock,
            new List<string>());
    }
}
