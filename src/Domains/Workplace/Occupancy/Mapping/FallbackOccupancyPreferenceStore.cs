namespace Acme.Workforce.Workplace.Occupancy.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback occupancy preferences by code.</summary>
public interface IFallbackOccupancyPreferenceStore
{
    FallbackOccupancyPreference? Find(string code);
    void Save(FallbackOccupancyPreference item);
    bool Remove(string code);
    List<FallbackOccupancyPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackOccupancyPreferenceStore(Dictionary<string, FallbackOccupancyPreference> items, List<string> order) : IFallbackOccupancyPreferenceStore
{
    public Dictionary<string, FallbackOccupancyPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackOccupancyPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackOccupancyPreference item)
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

    public List<FallbackOccupancyPreference> List()
    {
        var outValue = new List<FallbackOccupancyPreference>();
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
public sealed class CachedFallbackOccupancyPreferenceStore(MemoryFallbackOccupancyPreferenceStore inner, FallbackOccupancyPreferenceCache cache, int hits, int misses) : IFallbackOccupancyPreferenceStore
{
    public MemoryFallbackOccupancyPreferenceStore Inner { get; set; } = inner;
    public FallbackOccupancyPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackOccupancyPreference? Find(string code)
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

    public void Save(FallbackOccupancyPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackOccupancyPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback occupancy preferences. Empty fields match all.</summary>
public sealed record FallbackOccupancyPreferenceQuery(string Label, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackOccupancyPreference item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackOccupancyPreference> Apply(List<FallbackOccupancyPreference> items)
    {
        var matched = Collections.Filter(items, (FallbackOccupancyPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback occupancy preference store.</summary>
public static class FallbackOccupancyPreferenceStore
{
    public static IFallbackOccupancyPreferenceStore NewMemoryFallbackOccupancyPreferenceStore()
    {
        return new MemoryFallbackOccupancyPreferenceStore(
            new Dictionary<string, FallbackOccupancyPreference>(),
            new List<string>());
    }

    public static IFallbackOccupancyPreferenceStore NewCachedFallbackOccupancyPreferenceStore(MemoryFallbackOccupancyPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackOccupancyPreferenceStore(
            inner,
            FallbackOccupancyPreferenceCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<FallbackOccupancyPreference> FindFallbackOccupancyPreferences(IFallbackOccupancyPreferenceStore store, FallbackOccupancyPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
