namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual career path digests by code.</summary>
public interface IManualCareerPathDigestStore
{
    ManualCareerPathDigest? Find(string code);
    void Save(ManualCareerPathDigest item);
    bool Remove(string code);
    List<ManualCareerPathDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualCareerPathDigestStore(Dictionary<string, ManualCareerPathDigest> items, List<string> order) : IManualCareerPathDigestStore
{
    public Dictionary<string, ManualCareerPathDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualCareerPathDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualCareerPathDigest item)
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

    public List<ManualCareerPathDigest> List()
    {
        var outValue = new List<ManualCareerPathDigest>();
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
public sealed class CachedManualCareerPathDigestStore(MemoryManualCareerPathDigestStore inner, ManualCareerPathDigestCache cache, int hits, int misses) : IManualCareerPathDigestStore
{
    public MemoryManualCareerPathDigestStore Inner { get; set; } = inner;
    public ManualCareerPathDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualCareerPathDigest? Find(string code)
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

    public void Save(ManualCareerPathDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualCareerPathDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual career path digests. Empty fields match all.</summary>
public sealed record ManualCareerPathDigestQuery(string Locale, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ManualCareerPathDigest item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualCareerPathDigest> Apply(List<ManualCareerPathDigest> items)
    {
        var matched = Collections.Filter(items, (ManualCareerPathDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual career path digest store.</summary>
public static class ManualCareerPathDigestStore
{
    public static IManualCareerPathDigestStore NewMemoryManualCareerPathDigestStore()
    {
        return new MemoryManualCareerPathDigestStore(
            new Dictionary<string, ManualCareerPathDigest>(),
            new List<string>());
    }

    public static IManualCareerPathDigestStore NewCachedManualCareerPathDigestStore(MemoryManualCareerPathDigestStore inner, int ttl, IClock clock)
    {
        return new CachedManualCareerPathDigestStore(
            inner,
            ManualCareerPathDigestCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<ManualCareerPathDigest> FindManualCareerPathDigests(IManualCareerPathDigestStore store, ManualCareerPathDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
