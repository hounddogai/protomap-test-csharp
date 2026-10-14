namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered bonus pool quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredBonusPoolQuotaManager(ITieredBonusPoolQuotaStore Store, TieredBonusPoolQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered bonus pool quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredBonusPoolQuotaStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered bonus pool quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredBonusPoolQuotaStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of tiered bonus pool quota workflow.</summary>
public static class TieredBonusPoolQuotaWorkflow
{
    public static TieredBonusPoolQuotaManager NewTieredBonusPoolQuotaManager(ITieredBonusPoolQuotaStore store, IClock clock)
    {
        return new TieredBonusPoolQuotaManager(
            store,
            TieredBonusPoolQuotaRules.DefaultTieredBonusPoolQuotaValidator(),
            clock,
            new List<string>());
    }
}
