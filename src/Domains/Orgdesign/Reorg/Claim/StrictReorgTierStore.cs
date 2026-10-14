namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict reorg tiers by code.</summary>
public interface IStrictReorgTierStore
{
    StrictReorgTier? Find(string code);
    void Save(StrictReorgTier item);
    bool Remove(string code);
    List<StrictReorgTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictReorgTierStore(Dictionary<string, StrictReorgTier> items, List<string> order) : IStrictReorgTierStore
{
    public Dictionary<string, StrictReorgTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictReorgTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictReorgTier item)
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

    public List<StrictReorgTier> List()
    {
        var outValue = new List<StrictReorgTier>();
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
public sealed class CachedStrictReorgTierStore(MemoryStrictReorgTierStore inner, StrictReorgTierCache cache, int hits, int misses) : IStrictReorgTierStore
{
    public MemoryStrictReorgTierStore Inner { get; set; } = inner;
    public StrictReorgTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictReorgTier? Find(string code)
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

    public void Save(StrictReorgTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictReorgTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict reorg tiers. Empty fields match all.</summary>
public sealed record StrictReorgTierQuery(string Source, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictReorgTier item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<StrictReorgTier> Apply(List<StrictReorgTier> items)
    {
        var matched = Collections.Filter(items, (StrictReorgTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict reorg tier store.</summary>
public static class StrictReorgTierStore
{
    public static IStrictReorgTierStore NewMemoryStrictReorgTierStore()
    {
        return new MemoryStrictReorgTierStore(new Dictionary<string, StrictReorgTier>(), new List<string>());
    }

    public static IStrictReorgTierStore NewCachedStrictReorgTierStore(MemoryStrictReorgTierStore inner, int ttl, IClock clock)
    {
        return new CachedStrictReorgTierStore(inner, StrictReorgTierCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<StrictReorgTier> FindStrictReorgTiers(IStrictReorgTierStore store, StrictReorgTierQuery query)
    {
        return query.Apply(store.List());
    }
}
