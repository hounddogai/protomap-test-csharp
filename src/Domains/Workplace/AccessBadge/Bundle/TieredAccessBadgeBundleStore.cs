namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered access badge bundles by code.</summary>
public interface ITieredAccessBadgeBundleStore
{
    TieredAccessBadgeBundle? Find(string code);
    void Save(TieredAccessBadgeBundle item);
    bool Remove(string code);
    List<TieredAccessBadgeBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAccessBadgeBundleStore(Dictionary<string, TieredAccessBadgeBundle> items, List<string> order) : ITieredAccessBadgeBundleStore
{
    public Dictionary<string, TieredAccessBadgeBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAccessBadgeBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAccessBadgeBundle item)
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

    public List<TieredAccessBadgeBundle> List()
    {
        var outValue = new List<TieredAccessBadgeBundle>();
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
public sealed class CachedTieredAccessBadgeBundleStore(MemoryTieredAccessBadgeBundleStore inner, TieredAccessBadgeBundleCache cache, int hits, int misses) : ITieredAccessBadgeBundleStore
{
    public MemoryTieredAccessBadgeBundleStore Inner { get; set; } = inner;
    public TieredAccessBadgeBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAccessBadgeBundle? Find(string code)
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

    public void Save(TieredAccessBadgeBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAccessBadgeBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered access badge bundles. Empty fields match all.</summary>
public sealed record TieredAccessBadgeBundleQuery(string Category, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAccessBadgeBundle item)
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

    public List<TieredAccessBadgeBundle> Apply(List<TieredAccessBadgeBundle> items)
    {
        var matched = Collections.Filter(items, (TieredAccessBadgeBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered access badge bundle store.</summary>
public static class TieredAccessBadgeBundleStore
{
    public static ITieredAccessBadgeBundleStore NewMemoryTieredAccessBadgeBundleStore()
    {
        return new MemoryTieredAccessBadgeBundleStore(
            new Dictionary<string, TieredAccessBadgeBundle>(),
            new List<string>());
    }

    public static ITieredAccessBadgeBundleStore NewCachedTieredAccessBadgeBundleStore(MemoryTieredAccessBadgeBundleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAccessBadgeBundleStore(
            inner,
            TieredAccessBadgeBundleCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredAccessBadgeBundle> FindTieredAccessBadgeBundles(ITieredAccessBadgeBundleStore store, TieredAccessBadgeBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
