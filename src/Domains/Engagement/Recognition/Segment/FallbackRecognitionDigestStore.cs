namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback recognition digests by code.</summary>
public interface IFallbackRecognitionDigestStore
{
    FallbackRecognitionDigest? Find(string code);
    void Save(FallbackRecognitionDigest item);
    bool Remove(string code);
    List<FallbackRecognitionDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackRecognitionDigestStore(Dictionary<string, FallbackRecognitionDigest> items, List<string> order) : IFallbackRecognitionDigestStore
{
    public Dictionary<string, FallbackRecognitionDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackRecognitionDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackRecognitionDigest item)
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

    public List<FallbackRecognitionDigest> List()
    {
        var outValue = new List<FallbackRecognitionDigest>();
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
public sealed class CachedFallbackRecognitionDigestStore(MemoryFallbackRecognitionDigestStore inner, FallbackRecognitionDigestCache cache, int hits, int misses) : IFallbackRecognitionDigestStore
{
    public MemoryFallbackRecognitionDigestStore Inner { get; set; } = inner;
    public FallbackRecognitionDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackRecognitionDigest? Find(string code)
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

    public void Save(FallbackRecognitionDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackRecognitionDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback recognition digests. Empty fields match all.</summary>
public sealed record FallbackRecognitionDigestQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackRecognitionDigest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackRecognitionDigest> Apply(List<FallbackRecognitionDigest> items)
    {
        var matched = Collections.Filter(items, (FallbackRecognitionDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback recognition digest store.</summary>
public static class FallbackRecognitionDigestStore
{
    public static IFallbackRecognitionDigestStore NewMemoryFallbackRecognitionDigestStore()
    {
        return new MemoryFallbackRecognitionDigestStore(
            new Dictionary<string, FallbackRecognitionDigest>(),
            new List<string>());
    }

    public static IFallbackRecognitionDigestStore NewCachedFallbackRecognitionDigestStore(MemoryFallbackRecognitionDigestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackRecognitionDigestStore(
            inner,
            FallbackRecognitionDigestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackRecognitionDigest> FindFallbackRecognitionDigests(IFallbackRecognitionDigestStore store, FallbackRecognitionDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
