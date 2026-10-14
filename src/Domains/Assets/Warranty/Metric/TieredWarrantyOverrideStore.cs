namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered warranty overrides by code.</summary>
public interface ITieredWarrantyOverrideStore
{
    TieredWarrantyOverride? Find(string code);
    void Save(TieredWarrantyOverride item);
    bool Remove(string code);
    List<TieredWarrantyOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredWarrantyOverrideStore(Dictionary<string, TieredWarrantyOverride> items, List<string> order) : ITieredWarrantyOverrideStore
{
    public Dictionary<string, TieredWarrantyOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredWarrantyOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredWarrantyOverride item)
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

    public List<TieredWarrantyOverride> List()
    {
        var outValue = new List<TieredWarrantyOverride>();
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
public sealed class CachedTieredWarrantyOverrideStore(MemoryTieredWarrantyOverrideStore inner, TieredWarrantyOverrideCache cache, int hits, int misses) : ITieredWarrantyOverrideStore
{
    public MemoryTieredWarrantyOverrideStore Inner { get; set; } = inner;
    public TieredWarrantyOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredWarrantyOverride? Find(string code)
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

    public void Save(TieredWarrantyOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredWarrantyOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered warranty overrides. Empty fields match all.</summary>
public sealed record TieredWarrantyOverrideQuery(string Source, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(TieredWarrantyOverride item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredWarrantyOverride> Apply(List<TieredWarrantyOverride> items)
    {
        var matched = Collections.Filter(items, (TieredWarrantyOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered warranty override store.</summary>
public static class TieredWarrantyOverrideStore
{
    public static ITieredWarrantyOverrideStore NewMemoryTieredWarrantyOverrideStore()
    {
        return new MemoryTieredWarrantyOverrideStore(
            new Dictionary<string, TieredWarrantyOverride>(),
            new List<string>());
    }

    public static ITieredWarrantyOverrideStore NewCachedTieredWarrantyOverrideStore(MemoryTieredWarrantyOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedTieredWarrantyOverrideStore(
            inner,
            TieredWarrantyOverrideCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredWarrantyOverride> FindTieredWarrantyOverrides(ITieredWarrantyOverrideStore store, TieredWarrantyOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
