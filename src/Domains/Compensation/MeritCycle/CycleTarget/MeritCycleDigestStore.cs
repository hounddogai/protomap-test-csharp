namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle digests by code.</summary>
public interface IMeritCycleDigestStore
{
    MeritCycleDigest? Find(string code);
    void Save(MeritCycleDigest item);
    bool Remove(string code);
    List<MeritCycleDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleDigestStore(Dictionary<string, MeritCycleDigest> items, List<string> order) : IMeritCycleDigestStore
{
    public Dictionary<string, MeritCycleDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleDigest item)
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

    public List<MeritCycleDigest> List()
    {
        var outValue = new List<MeritCycleDigest>();
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
public sealed class CachedMeritCycleDigestStore(MemoryMeritCycleDigestStore inner, MeritCycleDigestCache cache, int hits, int misses) : IMeritCycleDigestStore
{
    public MemoryMeritCycleDigestStore Inner { get; set; } = inner;
    public MeritCycleDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleDigest? Find(string code)
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

    public void Save(MeritCycleDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle digests. Empty fields match all.</summary>
public sealed record MeritCycleDigestQuery(string Category, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleDigest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleDigest> Apply(List<MeritCycleDigest> items)
    {
        var matched = Collections.Filter(items, (MeritCycleDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle digest store.</summary>
public static class MeritCycleDigestStore
{
    public static IMeritCycleDigestStore NewMemoryMeritCycleDigestStore()
    {
        return new MemoryMeritCycleDigestStore(new Dictionary<string, MeritCycleDigest>(), new List<string>());
    }

    public static IMeritCycleDigestStore NewCachedMeritCycleDigestStore(MemoryMeritCycleDigestStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleDigestStore(inner, MeritCycleDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCycleDigest> FindMeritCycleDigests(IMeritCycleDigestStore store, MeritCycleDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
