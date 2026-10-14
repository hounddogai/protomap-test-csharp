namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback recognition overrides by code.</summary>
public interface IFallbackRecognitionOverrideStore
{
    FallbackRecognitionOverride? Find(string code);
    void Save(FallbackRecognitionOverride item);
    bool Remove(string code);
    List<FallbackRecognitionOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackRecognitionOverrideStore(Dictionary<string, FallbackRecognitionOverride> items, List<string> order) : IFallbackRecognitionOverrideStore
{
    public Dictionary<string, FallbackRecognitionOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackRecognitionOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackRecognitionOverride item)
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

    public List<FallbackRecognitionOverride> List()
    {
        var outValue = new List<FallbackRecognitionOverride>();
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
public sealed class CachedFallbackRecognitionOverrideStore(MemoryFallbackRecognitionOverrideStore inner, FallbackRecognitionOverrideCache cache, int hits, int misses) : IFallbackRecognitionOverrideStore
{
    public MemoryFallbackRecognitionOverrideStore Inner { get; set; } = inner;
    public FallbackRecognitionOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackRecognitionOverride? Find(string code)
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

    public void Save(FallbackRecognitionOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackRecognitionOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback recognition overrides. Empty fields match all.</summary>
public sealed record FallbackRecognitionOverrideQuery(string Region, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackRecognitionOverride item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackRecognitionOverride> Apply(List<FallbackRecognitionOverride> items)
    {
        var matched = Collections.Filter(items, (FallbackRecognitionOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback recognition override store.</summary>
public static class FallbackRecognitionOverrideStore
{
    public static IFallbackRecognitionOverrideStore NewMemoryFallbackRecognitionOverrideStore()
    {
        return new MemoryFallbackRecognitionOverrideStore(
            new Dictionary<string, FallbackRecognitionOverride>(),
            new List<string>());
    }

    public static IFallbackRecognitionOverrideStore NewCachedFallbackRecognitionOverrideStore(MemoryFallbackRecognitionOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackRecognitionOverrideStore(
            inner,
            FallbackRecognitionOverrideCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackRecognitionOverride> FindFallbackRecognitionOverrides(IFallbackRecognitionOverrideStore store, FallbackRecognitionOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
