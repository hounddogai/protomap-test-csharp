namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg tiers by code.</summary>
public interface IReorgTierStore
{
    ReorgTier? Find(string code);
    void Save(ReorgTier item);
    bool Remove(string code);
    List<ReorgTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgTierStore(Dictionary<string, ReorgTier> items, List<string> order) : IReorgTierStore
{
    public Dictionary<string, ReorgTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgTier item)
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

    public List<ReorgTier> List()
    {
        var outValue = new List<ReorgTier>();
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
public sealed class CachedReorgTierStore(MemoryReorgTierStore inner, ReorgTierCache cache, int hits, int misses) : IReorgTierStore
{
    public MemoryReorgTierStore Inner { get; set; } = inner;
    public ReorgTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgTier? Find(string code)
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

    public void Save(ReorgTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg tiers. Empty fields match all.</summary>
public sealed record ReorgTierQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgTier item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgTier> Apply(List<ReorgTier> items)
    {
        var matched = Collections.Filter(items, (ReorgTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg tier store.</summary>
public static class ReorgTierStore
{
    public static IReorgTierStore NewMemoryReorgTierStore()
    {
        return new MemoryReorgTierStore(new Dictionary<string, ReorgTier>(), new List<string>());
    }

    public static IReorgTierStore NewCachedReorgTierStore(MemoryReorgTierStore inner, int ttl, IClock clock)
    {
        return new CachedReorgTierStore(inner, ReorgTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgTier> FindReorgTiers(IReorgTierStore store, ReorgTierQuery query)
    {
        return query.Apply(store.List());
    }
}
