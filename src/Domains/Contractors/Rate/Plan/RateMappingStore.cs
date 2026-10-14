namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate mappings by code.</summary>
public interface IRateMappingStore
{
    RateMapping? Find(string code);
    void Save(RateMapping item);
    bool Remove(string code);
    List<RateMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateMappingStore(Dictionary<string, RateMapping> items, List<string> order) : IRateMappingStore
{
    public Dictionary<string, RateMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateMapping item)
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

    public List<RateMapping> List()
    {
        var outValue = new List<RateMapping>();
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
public sealed class CachedRateMappingStore(MemoryRateMappingStore inner, RateMappingCache cache, int hits, int misses) : IRateMappingStore
{
    public MemoryRateMappingStore Inner { get; set; } = inner;
    public RateMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateMapping? Find(string code)
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

    public void Save(RateMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate mappings. Empty fields match all.</summary>
public sealed record RateMappingQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(RateMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RateMapping> Apply(List<RateMapping> items)
    {
        var matched = Collections.Filter(items, (RateMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate mapping store.</summary>
public static class RateMappingStore
{
    public static IRateMappingStore NewMemoryRateMappingStore()
    {
        return new MemoryRateMappingStore(new Dictionary<string, RateMapping>(), new List<string>());
    }

    public static IRateMappingStore NewCachedRateMappingStore(MemoryRateMappingStore inner, int ttl, IClock clock)
    {
        return new CachedRateMappingStore(inner, RateMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RateMapping> FindRateMappings(IRateMappingStore store, RateMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
