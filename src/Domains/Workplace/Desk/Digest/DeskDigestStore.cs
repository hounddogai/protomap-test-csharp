namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk digests by code.</summary>
public interface IDeskDigestStore
{
    DeskDigest? Find(string code);
    void Save(DeskDigest item);
    bool Remove(string code);
    List<DeskDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskDigestStore(Dictionary<string, DeskDigest> items, List<string> order) : IDeskDigestStore
{
    public Dictionary<string, DeskDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskDigest item)
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

    public List<DeskDigest> List()
    {
        var outValue = new List<DeskDigest>();
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
public sealed class CachedDeskDigestStore(MemoryDeskDigestStore inner, DeskDigestCache cache, int hits, int misses) : IDeskDigestStore
{
    public MemoryDeskDigestStore Inner { get; set; } = inner;
    public DeskDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskDigest? Find(string code)
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

    public void Save(DeskDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk digests. Empty fields match all.</summary>
public sealed record DeskDigestQuery(string Region, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(DeskDigest item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<DeskDigest> Apply(List<DeskDigest> items)
    {
        var matched = Collections.Filter(items, (DeskDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk digest store.</summary>
public static class DeskDigestStore
{
    public static IDeskDigestStore NewMemoryDeskDigestStore()
    {
        return new MemoryDeskDigestStore(new Dictionary<string, DeskDigest>(), new List<string>());
    }

    public static IDeskDigestStore NewCachedDeskDigestStore(MemoryDeskDigestStore inner, int ttl, IClock clock)
    {
        return new CachedDeskDigestStore(inner, DeskDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskDigest> FindDeskDigests(IDeskDigestStore store, DeskDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
