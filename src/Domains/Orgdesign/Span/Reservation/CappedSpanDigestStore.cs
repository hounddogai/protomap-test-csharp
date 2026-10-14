namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped span digests by code.</summary>
public interface ICappedSpanDigestStore
{
    CappedSpanDigest? Find(string code);
    void Save(CappedSpanDigest item);
    bool Remove(string code);
    List<CappedSpanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSpanDigestStore(Dictionary<string, CappedSpanDigest> items, List<string> order) : ICappedSpanDigestStore
{
    public Dictionary<string, CappedSpanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSpanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSpanDigest item)
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

    public List<CappedSpanDigest> List()
    {
        var outValue = new List<CappedSpanDigest>();
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
public sealed class CachedCappedSpanDigestStore(MemoryCappedSpanDigestStore inner, CappedSpanDigestCache cache, int hits, int misses) : ICappedSpanDigestStore
{
    public MemoryCappedSpanDigestStore Inner { get; set; } = inner;
    public CappedSpanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSpanDigest? Find(string code)
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

    public void Save(CappedSpanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSpanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped span digests. Empty fields match all.</summary>
public sealed record CappedSpanDigestQuery(string Department, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSpanDigest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSpanDigest> Apply(List<CappedSpanDigest> items)
    {
        var matched = Collections.Filter(items, (CappedSpanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped span digest store.</summary>
public static class CappedSpanDigestStore
{
    public static ICappedSpanDigestStore NewMemoryCappedSpanDigestStore()
    {
        return new MemoryCappedSpanDigestStore(new Dictionary<string, CappedSpanDigest>(), new List<string>());
    }

    public static ICappedSpanDigestStore NewCachedCappedSpanDigestStore(MemoryCappedSpanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSpanDigestStore(inner, CappedSpanDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<CappedSpanDigest> FindCappedSpanDigests(ICappedSpanDigestStore store, CappedSpanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
