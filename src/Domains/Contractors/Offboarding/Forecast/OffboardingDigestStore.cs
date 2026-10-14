namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding digests by code.</summary>
public interface IOffboardingDigestStore
{
    OffboardingDigest? Find(string code);
    void Save(OffboardingDigest item);
    bool Remove(string code);
    List<OffboardingDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingDigestStore(Dictionary<string, OffboardingDigest> items, List<string> order) : IOffboardingDigestStore
{
    public Dictionary<string, OffboardingDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingDigest item)
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

    public List<OffboardingDigest> List()
    {
        var outValue = new List<OffboardingDigest>();
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
public sealed class CachedOffboardingDigestStore(MemoryOffboardingDigestStore inner, OffboardingDigestCache cache, int hits, int misses) : IOffboardingDigestStore
{
    public MemoryOffboardingDigestStore Inner { get; set; } = inner;
    public OffboardingDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingDigest? Find(string code)
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

    public void Save(OffboardingDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding digests. Empty fields match all.</summary>
public sealed record OffboardingDigestQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingDigest> Apply(List<OffboardingDigest> items)
    {
        var matched = Collections.Filter(items, (OffboardingDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding digest store.</summary>
public static class OffboardingDigestStore
{
    public static IOffboardingDigestStore NewMemoryOffboardingDigestStore()
    {
        return new MemoryOffboardingDigestStore(new Dictionary<string, OffboardingDigest>(), new List<string>());
    }

    public static IOffboardingDigestStore NewCachedOffboardingDigestStore(MemoryOffboardingDigestStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingDigestStore(inner, OffboardingDigestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<OffboardingDigest> FindOffboardingDigests(IOffboardingDigestStore store, OffboardingDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
