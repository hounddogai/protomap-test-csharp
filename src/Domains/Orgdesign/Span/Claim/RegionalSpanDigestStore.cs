namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional span digests by code.</summary>
public interface IRegionalSpanDigestStore
{
    RegionalSpanDigest? Find(string code);
    void Save(RegionalSpanDigest item);
    bool Remove(string code);
    List<RegionalSpanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSpanDigestStore(Dictionary<string, RegionalSpanDigest> items, List<string> order) : IRegionalSpanDigestStore
{
    public Dictionary<string, RegionalSpanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSpanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSpanDigest item)
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

    public List<RegionalSpanDigest> List()
    {
        var outValue = new List<RegionalSpanDigest>();
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
public sealed class CachedRegionalSpanDigestStore(MemoryRegionalSpanDigestStore inner, RegionalSpanDigestCache cache, int hits, int misses) : IRegionalSpanDigestStore
{
    public MemoryRegionalSpanDigestStore Inner { get; set; } = inner;
    public RegionalSpanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSpanDigest? Find(string code)
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

    public void Save(RegionalSpanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSpanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional span digests. Empty fields match all.</summary>
public sealed record RegionalSpanDigestQuery(string Segment, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSpanDigest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSpanDigest> Apply(List<RegionalSpanDigest> items)
    {
        var matched = Collections.Filter(items, (RegionalSpanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional span digest store.</summary>
public static class RegionalSpanDigestStore
{
    public static IRegionalSpanDigestStore NewMemoryRegionalSpanDigestStore()
    {
        return new MemoryRegionalSpanDigestStore(new Dictionary<string, RegionalSpanDigest>(), new List<string>());
    }

    public static IRegionalSpanDigestStore NewCachedRegionalSpanDigestStore(MemoryRegionalSpanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSpanDigestStore(inner, RegionalSpanDigestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RegionalSpanDigest> FindRegionalSpanDigests(IRegionalSpanDigestStore store, RegionalSpanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
