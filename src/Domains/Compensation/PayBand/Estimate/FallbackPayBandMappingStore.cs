namespace Acme.Workforce.Compensation.PayBand.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback pay band mappings by code.</summary>
public interface IFallbackPayBandMappingStore
{
    FallbackPayBandMapping? Find(string code);
    void Save(FallbackPayBandMapping item);
    bool Remove(string code);
    List<FallbackPayBandMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackPayBandMappingStore(Dictionary<string, FallbackPayBandMapping> items, List<string> order) : IFallbackPayBandMappingStore
{
    public Dictionary<string, FallbackPayBandMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackPayBandMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackPayBandMapping item)
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

    public List<FallbackPayBandMapping> List()
    {
        var outValue = new List<FallbackPayBandMapping>();
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
public sealed class CachedFallbackPayBandMappingStore(MemoryFallbackPayBandMappingStore inner, FallbackPayBandMappingCache cache, int hits, int misses) : IFallbackPayBandMappingStore
{
    public MemoryFallbackPayBandMappingStore Inner { get; set; } = inner;
    public FallbackPayBandMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackPayBandMapping? Find(string code)
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

    public void Save(FallbackPayBandMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackPayBandMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback pay band mappings. Empty fields match all.</summary>
public sealed record FallbackPayBandMappingQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackPayBandMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackPayBandMapping> Apply(List<FallbackPayBandMapping> items)
    {
        var matched = Collections.Filter(items, (FallbackPayBandMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback pay band mapping store.</summary>
public static class FallbackPayBandMappingStore
{
    public static IFallbackPayBandMappingStore NewMemoryFallbackPayBandMappingStore()
    {
        return new MemoryFallbackPayBandMappingStore(
            new Dictionary<string, FallbackPayBandMapping>(),
            new List<string>());
    }

    public static IFallbackPayBandMappingStore NewCachedFallbackPayBandMappingStore(MemoryFallbackPayBandMappingStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackPayBandMappingStore(
            inner,
            FallbackPayBandMappingCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackPayBandMapping> FindFallbackPayBandMappings(IFallbackPayBandMappingStore store, FallbackPayBandMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
