namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy occupancy digests by code.</summary>
public interface ILegacyOccupancyDigestStore
{
    LegacyOccupancyDigest? Find(string code);
    void Save(LegacyOccupancyDigest item);
    bool Remove(string code);
    List<LegacyOccupancyDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyOccupancyDigestStore(Dictionary<string, LegacyOccupancyDigest> items, List<string> order) : ILegacyOccupancyDigestStore
{
    public Dictionary<string, LegacyOccupancyDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyOccupancyDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyOccupancyDigest item)
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

    public List<LegacyOccupancyDigest> List()
    {
        var outValue = new List<LegacyOccupancyDigest>();
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
public sealed class CachedLegacyOccupancyDigestStore(MemoryLegacyOccupancyDigestStore inner, LegacyOccupancyDigestCache cache, int hits, int misses) : ILegacyOccupancyDigestStore
{
    public MemoryLegacyOccupancyDigestStore Inner { get; set; } = inner;
    public LegacyOccupancyDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyOccupancyDigest? Find(string code)
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

    public void Save(LegacyOccupancyDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyOccupancyDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy occupancy digests. Empty fields match all.</summary>
public sealed record LegacyOccupancyDigestQuery(string Name, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyOccupancyDigest item)
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

    public List<LegacyOccupancyDigest> Apply(List<LegacyOccupancyDigest> items)
    {
        var matched = Collections.Filter(items, (LegacyOccupancyDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy occupancy digest store.</summary>
public static class LegacyOccupancyDigestStore
{
    public static ILegacyOccupancyDigestStore NewMemoryLegacyOccupancyDigestStore()
    {
        return new MemoryLegacyOccupancyDigestStore(
            new Dictionary<string, LegacyOccupancyDigest>(),
            new List<string>());
    }

    public static ILegacyOccupancyDigestStore NewCachedLegacyOccupancyDigestStore(MemoryLegacyOccupancyDigestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyOccupancyDigestStore(inner, LegacyOccupancyDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyOccupancyDigest> FindLegacyOccupancyDigests(ILegacyOccupancyDigestStore store, LegacyOccupancyDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
