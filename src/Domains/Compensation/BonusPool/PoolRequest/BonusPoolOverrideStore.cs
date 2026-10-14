namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps bonus pool overrides by code.</summary>
public interface IBonusPoolOverrideStore
{
    BonusPoolOverride? Find(string code);
    void Save(BonusPoolOverride item);
    bool Remove(string code);
    List<BonusPoolOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBonusPoolOverrideStore(Dictionary<string, BonusPoolOverride> items, List<string> order) : IBonusPoolOverrideStore
{
    public Dictionary<string, BonusPoolOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BonusPoolOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BonusPoolOverride item)
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

    public List<BonusPoolOverride> List()
    {
        var outValue = new List<BonusPoolOverride>();
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
public sealed class CachedBonusPoolOverrideStore(MemoryBonusPoolOverrideStore inner, BonusPoolOverrideCache cache, int hits, int misses) : IBonusPoolOverrideStore
{
    public MemoryBonusPoolOverrideStore Inner { get; set; } = inner;
    public BonusPoolOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BonusPoolOverride? Find(string code)
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

    public void Save(BonusPoolOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BonusPoolOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects bonus pool overrides. Empty fields match all.</summary>
public sealed record BonusPoolOverrideQuery(string Category, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(BonusPoolOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BonusPoolOverride> Apply(List<BonusPoolOverride> items)
    {
        var matched = Collections.Filter(items, (BonusPoolOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of bonus pool override store.</summary>
public static class BonusPoolOverrideStore
{
    public static IBonusPoolOverrideStore NewMemoryBonusPoolOverrideStore()
    {
        return new MemoryBonusPoolOverrideStore(new Dictionary<string, BonusPoolOverride>(), new List<string>());
    }

    public static IBonusPoolOverrideStore NewCachedBonusPoolOverrideStore(MemoryBonusPoolOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedBonusPoolOverrideStore(inner, BonusPoolOverrideCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BonusPoolOverride> FindBonusPoolOverrides(IBonusPoolOverrideStore store, BonusPoolOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
