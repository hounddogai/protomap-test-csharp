namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position digests by code.</summary>
public interface IPositionDigestStore
{
    PositionDigest? Find(string code);
    void Save(PositionDigest item);
    bool Remove(string code);
    List<PositionDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionDigestStore(Dictionary<string, PositionDigest> items, List<string> order) : IPositionDigestStore
{
    public Dictionary<string, PositionDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionDigest item)
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

    public List<PositionDigest> List()
    {
        var outValue = new List<PositionDigest>();
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
public sealed class CachedPositionDigestStore(MemoryPositionDigestStore inner, PositionDigestCache cache, int hits, int misses) : IPositionDigestStore
{
    public MemoryPositionDigestStore Inner { get; set; } = inner;
    public PositionDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionDigest? Find(string code)
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

    public void Save(PositionDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position digests. Empty fields match all.</summary>
public sealed record PositionDigestQuery(string Locale, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(PositionDigest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<PositionDigest> Apply(List<PositionDigest> items)
    {
        var matched = Collections.Filter(items, (PositionDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position digest store.</summary>
public static class PositionDigestStore
{
    public static IPositionDigestStore NewMemoryPositionDigestStore()
    {
        return new MemoryPositionDigestStore(new Dictionary<string, PositionDigest>(), new List<string>());
    }

    public static IPositionDigestStore NewCachedPositionDigestStore(MemoryPositionDigestStore inner, int ttl, IClock clock)
    {
        return new CachedPositionDigestStore(inner, PositionDigestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PositionDigest> FindPositionDigests(IPositionDigestStore store, PositionDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
