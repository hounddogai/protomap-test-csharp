namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending sentiment digests by code.</summary>
public interface IPendingSentimentDigestStore
{
    PendingSentimentDigest? Find(string code);
    void Save(PendingSentimentDigest item);
    bool Remove(string code);
    List<PendingSentimentDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSentimentDigestStore(Dictionary<string, PendingSentimentDigest> items, List<string> order) : IPendingSentimentDigestStore
{
    public Dictionary<string, PendingSentimentDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSentimentDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSentimentDigest item)
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

    public List<PendingSentimentDigest> List()
    {
        var outValue = new List<PendingSentimentDigest>();
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
public sealed class CachedPendingSentimentDigestStore(MemoryPendingSentimentDigestStore inner, PendingSentimentDigestCache cache, int hits, int misses) : IPendingSentimentDigestStore
{
    public MemoryPendingSentimentDigestStore Inner { get; set; } = inner;
    public PendingSentimentDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSentimentDigest? Find(string code)
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

    public void Save(PendingSentimentDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSentimentDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending sentiment digests. Empty fields match all.</summary>
public sealed record PendingSentimentDigestQuery(string Segment, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSentimentDigest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<PendingSentimentDigest> Apply(List<PendingSentimentDigest> items)
    {
        var matched = Collections.Filter(items, (PendingSentimentDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending sentiment digest store.</summary>
public static class PendingSentimentDigestStore
{
    public static IPendingSentimentDigestStore NewMemoryPendingSentimentDigestStore()
    {
        return new MemoryPendingSentimentDigestStore(
            new Dictionary<string, PendingSentimentDigest>(),
            new List<string>());
    }

    public static IPendingSentimentDigestStore NewCachedPendingSentimentDigestStore(MemoryPendingSentimentDigestStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSentimentDigestStore(
            inner,
            PendingSentimentDigestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PendingSentimentDigest> FindPendingSentimentDigests(IPendingSentimentDigestStore store, PendingSentimentDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
