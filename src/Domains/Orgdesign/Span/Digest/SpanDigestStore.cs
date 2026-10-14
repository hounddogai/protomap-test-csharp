namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span digests by code.</summary>
public interface ISpanDigestStore
{
    SpanDigest? Find(string code);
    void Save(SpanDigest item);
    bool Remove(string code);
    List<SpanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanDigestStore(Dictionary<string, SpanDigest> items, List<string> order) : ISpanDigestStore
{
    public Dictionary<string, SpanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanDigest item)
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

    public List<SpanDigest> List()
    {
        var outValue = new List<SpanDigest>();
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
public sealed class CachedSpanDigestStore(MemorySpanDigestStore inner, SpanDigestCache cache, int hits, int misses) : ISpanDigestStore
{
    public MemorySpanDigestStore Inner { get; set; } = inner;
    public SpanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanDigest? Find(string code)
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

    public void Save(SpanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span digests. Empty fields match all.</summary>
public sealed record SpanDigestQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(SpanDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SpanDigest> Apply(List<SpanDigest> items)
    {
        var matched = Collections.Filter(items, (SpanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span digest store.</summary>
public static class SpanDigestStore
{
    public static ISpanDigestStore NewMemorySpanDigestStore()
    {
        return new MemorySpanDigestStore(new Dictionary<string, SpanDigest>(), new List<string>());
    }

    public static ISpanDigestStore NewCachedSpanDigestStore(MemorySpanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedSpanDigestStore(inner, SpanDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SpanDigest> FindSpanDigests(ISpanDigestStore store, SpanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
