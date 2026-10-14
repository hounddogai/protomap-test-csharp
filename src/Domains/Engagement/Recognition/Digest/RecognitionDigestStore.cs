namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps recognition digests by code.</summary>
public interface IRecognitionDigestStore
{
    RecognitionDigest? Find(string code);
    void Save(RecognitionDigest item);
    bool Remove(string code);
    List<RecognitionDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRecognitionDigestStore(Dictionary<string, RecognitionDigest> items, List<string> order) : IRecognitionDigestStore
{
    public Dictionary<string, RecognitionDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RecognitionDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RecognitionDigest item)
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

    public List<RecognitionDigest> List()
    {
        var outValue = new List<RecognitionDigest>();
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
public sealed class CachedRecognitionDigestStore(MemoryRecognitionDigestStore inner, RecognitionDigestCache cache, int hits, int misses) : IRecognitionDigestStore
{
    public MemoryRecognitionDigestStore Inner { get; set; } = inner;
    public RecognitionDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RecognitionDigest? Find(string code)
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

    public void Save(RecognitionDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RecognitionDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects recognition digests. Empty fields match all.</summary>
public sealed record RecognitionDigestQuery(string Segment, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(RecognitionDigest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RecognitionDigest> Apply(List<RecognitionDigest> items)
    {
        var matched = Collections.Filter(items, (RecognitionDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of recognition digest store.</summary>
public static class RecognitionDigestStore
{
    public static IRecognitionDigestStore NewMemoryRecognitionDigestStore()
    {
        return new MemoryRecognitionDigestStore(new Dictionary<string, RecognitionDigest>(), new List<string>());
    }

    public static IRecognitionDigestStore NewCachedRecognitionDigestStore(MemoryRecognitionDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRecognitionDigestStore(inner, RecognitionDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RecognitionDigest> FindRecognitionDigests(IRecognitionDigestStore store, RecognitionDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
