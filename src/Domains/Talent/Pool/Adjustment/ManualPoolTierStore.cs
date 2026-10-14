namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pool tiers by code.</summary>
public interface IManualPoolTierStore
{
    ManualPoolTier? Find(string code);
    void Save(ManualPoolTier item);
    bool Remove(string code);
    List<ManualPoolTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPoolTierStore(Dictionary<string, ManualPoolTier> items, List<string> order) : IManualPoolTierStore
{
    public Dictionary<string, ManualPoolTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPoolTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPoolTier item)
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

    public List<ManualPoolTier> List()
    {
        var outValue = new List<ManualPoolTier>();
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
public sealed class CachedManualPoolTierStore(MemoryManualPoolTierStore inner, ManualPoolTierCache cache, int hits, int misses) : IManualPoolTierStore
{
    public MemoryManualPoolTierStore Inner { get; set; } = inner;
    public ManualPoolTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPoolTier? Find(string code)
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

    public void Save(ManualPoolTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPoolTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pool tiers. Empty fields match all.</summary>
public sealed record ManualPoolTierQuery(string Segment, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPoolTier item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPoolTier> Apply(List<ManualPoolTier> items)
    {
        var matched = Collections.Filter(items, (ManualPoolTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pool tier store.</summary>
public static class ManualPoolTierStore
{
    public static IManualPoolTierStore NewMemoryManualPoolTierStore()
    {
        return new MemoryManualPoolTierStore(new Dictionary<string, ManualPoolTier>(), new List<string>());
    }

    public static IManualPoolTierStore NewCachedManualPoolTierStore(MemoryManualPoolTierStore inner, int ttl, IClock clock)
    {
        return new CachedManualPoolTierStore(inner, ManualPoolTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ManualPoolTier> FindManualPoolTiers(IManualPoolTierStore store, ManualPoolTierQuery query)
    {
        return query.Apply(store.List());
    }
}
