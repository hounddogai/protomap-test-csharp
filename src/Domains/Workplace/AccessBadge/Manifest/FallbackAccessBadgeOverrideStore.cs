namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback access badge overrides by code.</summary>
public interface IFallbackAccessBadgeOverrideStore
{
    FallbackAccessBadgeOverride? Find(string code);
    void Save(FallbackAccessBadgeOverride item);
    bool Remove(string code);
    List<FallbackAccessBadgeOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackAccessBadgeOverrideStore(Dictionary<string, FallbackAccessBadgeOverride> items, List<string> order) : IFallbackAccessBadgeOverrideStore
{
    public Dictionary<string, FallbackAccessBadgeOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackAccessBadgeOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackAccessBadgeOverride item)
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

    public List<FallbackAccessBadgeOverride> List()
    {
        var outValue = new List<FallbackAccessBadgeOverride>();
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
public sealed class CachedFallbackAccessBadgeOverrideStore(MemoryFallbackAccessBadgeOverrideStore inner, FallbackAccessBadgeOverrideCache cache, int hits, int misses) : IFallbackAccessBadgeOverrideStore
{
    public MemoryFallbackAccessBadgeOverrideStore Inner { get; set; } = inner;
    public FallbackAccessBadgeOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackAccessBadgeOverride? Find(string code)
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

    public void Save(FallbackAccessBadgeOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackAccessBadgeOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback access badge overrides. Empty fields match all.</summary>
public sealed record FallbackAccessBadgeOverrideQuery(string Category, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackAccessBadgeOverride item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackAccessBadgeOverride> Apply(List<FallbackAccessBadgeOverride> items)
    {
        var matched = Collections.Filter(items, (FallbackAccessBadgeOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback access badge override store.</summary>
public static class FallbackAccessBadgeOverrideStore
{
    public static IFallbackAccessBadgeOverrideStore NewMemoryFallbackAccessBadgeOverrideStore()
    {
        return new MemoryFallbackAccessBadgeOverrideStore(
            new Dictionary<string, FallbackAccessBadgeOverride>(),
            new List<string>());
    }

    public static IFallbackAccessBadgeOverrideStore NewCachedFallbackAccessBadgeOverrideStore(MemoryFallbackAccessBadgeOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackAccessBadgeOverrideStore(
            inner,
            FallbackAccessBadgeOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackAccessBadgeOverride> FindFallbackAccessBadgeOverrides(IFallbackAccessBadgeOverrideStore store, FallbackAccessBadgeOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
