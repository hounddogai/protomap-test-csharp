namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback exception tiers by code.</summary>
public interface IFallbackExceptionTierStore
{
    FallbackExceptionTier? Find(string code);
    void Save(FallbackExceptionTier item);
    bool Remove(string code);
    List<FallbackExceptionTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackExceptionTierStore(Dictionary<string, FallbackExceptionTier> items, List<string> order) : IFallbackExceptionTierStore
{
    public Dictionary<string, FallbackExceptionTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackExceptionTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackExceptionTier item)
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

    public List<FallbackExceptionTier> List()
    {
        var outValue = new List<FallbackExceptionTier>();
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
public sealed class CachedFallbackExceptionTierStore(MemoryFallbackExceptionTierStore inner, FallbackExceptionTierCache cache, int hits, int misses) : IFallbackExceptionTierStore
{
    public MemoryFallbackExceptionTierStore Inner { get; set; } = inner;
    public FallbackExceptionTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackExceptionTier? Find(string code)
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

    public void Save(FallbackExceptionTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackExceptionTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback exception tiers. Empty fields match all.</summary>
public sealed record FallbackExceptionTierQuery(string Category, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackExceptionTier item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackExceptionTier> Apply(List<FallbackExceptionTier> items)
    {
        var matched = Collections.Filter(items, (FallbackExceptionTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback exception tier store.</summary>
public static class FallbackExceptionTierStore
{
    public static IFallbackExceptionTierStore NewMemoryFallbackExceptionTierStore()
    {
        return new MemoryFallbackExceptionTierStore(
            new Dictionary<string, FallbackExceptionTier>(),
            new List<string>());
    }

    public static IFallbackExceptionTierStore NewCachedFallbackExceptionTierStore(MemoryFallbackExceptionTierStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackExceptionTierStore(
            inner,
            FallbackExceptionTierCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackExceptionTier> FindFallbackExceptionTiers(IFallbackExceptionTierStore store, FallbackExceptionTierQuery query)
    {
        return query.Apply(store.List());
    }
}
