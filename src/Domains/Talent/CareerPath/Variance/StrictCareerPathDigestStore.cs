namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict career path digests by code.</summary>
public interface IStrictCareerPathDigestStore
{
    StrictCareerPathDigest? Find(string code);
    void Save(StrictCareerPathDigest item);
    bool Remove(string code);
    List<StrictCareerPathDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictCareerPathDigestStore(Dictionary<string, StrictCareerPathDigest> items, List<string> order) : IStrictCareerPathDigestStore
{
    public Dictionary<string, StrictCareerPathDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictCareerPathDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictCareerPathDigest item)
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

    public List<StrictCareerPathDigest> List()
    {
        var outValue = new List<StrictCareerPathDigest>();
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
public sealed class CachedStrictCareerPathDigestStore(MemoryStrictCareerPathDigestStore inner, StrictCareerPathDigestCache cache, int hits, int misses) : IStrictCareerPathDigestStore
{
    public MemoryStrictCareerPathDigestStore Inner { get; set; } = inner;
    public StrictCareerPathDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictCareerPathDigest? Find(string code)
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

    public void Save(StrictCareerPathDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictCareerPathDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict career path digests. Empty fields match all.</summary>
public sealed record StrictCareerPathDigestQuery(string Country, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(StrictCareerPathDigest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictCareerPathDigest> Apply(List<StrictCareerPathDigest> items)
    {
        var matched = Collections.Filter(items, (StrictCareerPathDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict career path digest store.</summary>
public static class StrictCareerPathDigestStore
{
    public static IStrictCareerPathDigestStore NewMemoryStrictCareerPathDigestStore()
    {
        return new MemoryStrictCareerPathDigestStore(
            new Dictionary<string, StrictCareerPathDigest>(),
            new List<string>());
    }

    public static IStrictCareerPathDigestStore NewCachedStrictCareerPathDigestStore(MemoryStrictCareerPathDigestStore inner, int ttl, IClock clock)
    {
        return new CachedStrictCareerPathDigestStore(
            inner,
            StrictCareerPathDigestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<StrictCareerPathDigest> FindStrictCareerPathDigests(IStrictCareerPathDigestStore store, StrictCareerPathDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
