namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy statement of work targets by code.</summary>
public interface ILegacyStatementOfWorkTargetStore
{
    LegacyStatementOfWorkTarget? Find(string code);
    void Save(LegacyStatementOfWorkTarget item);
    bool Remove(string code);
    List<LegacyStatementOfWorkTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyStatementOfWorkTargetStore(Dictionary<string, LegacyStatementOfWorkTarget> items, List<string> order) : ILegacyStatementOfWorkTargetStore
{
    public Dictionary<string, LegacyStatementOfWorkTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyStatementOfWorkTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyStatementOfWorkTarget item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<LegacyStatementOfWorkTarget> List()
    {
        var outValue = new List<LegacyStatementOfWorkTarget>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedLegacyStatementOfWorkTargetStore(MemoryLegacyStatementOfWorkTargetStore inner, LegacyStatementOfWorkTargetCache cache, int hits, int misses) : ILegacyStatementOfWorkTargetStore
{
    public MemoryLegacyStatementOfWorkTargetStore Inner { get; set; } = inner;
    public LegacyStatementOfWorkTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyStatementOfWorkTarget? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(LegacyStatementOfWorkTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyStatementOfWorkTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy statement of work targets. Empty fields match all.</summary>
public sealed record LegacyStatementOfWorkTargetQuery(string Team, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyStatementOfWorkTarget item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyStatementOfWorkTarget> Apply(List<LegacyStatementOfWorkTarget> items)
    {
        var matched = Collections.Filter(items, (LegacyStatementOfWorkTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy statement of work target store.</summary>
public static class LegacyStatementOfWorkTargetStore
{
    public static ILegacyStatementOfWorkTargetStore NewMemoryLegacyStatementOfWorkTargetStore()
    {
        return new MemoryLegacyStatementOfWorkTargetStore(
            new Dictionary<string, LegacyStatementOfWorkTarget>(),
            new List<string>());
    }

    public static ILegacyStatementOfWorkTargetStore NewCachedLegacyStatementOfWorkTargetStore(MemoryLegacyStatementOfWorkTargetStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyStatementOfWorkTargetStore(
            inner,
            LegacyStatementOfWorkTargetCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<LegacyStatementOfWorkTarget> FindLegacyStatementOfWorkTargets(ILegacyStatementOfWorkTargetStore store, LegacyStatementOfWorkTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
