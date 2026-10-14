namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped career path digests by code.</summary>
public interface ICappedCareerPathDigestStore
{
    CappedCareerPathDigest? Find(string code);
    void Save(CappedCareerPathDigest item);
    bool Remove(string code);
    List<CappedCareerPathDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedCareerPathDigestStore(Dictionary<string, CappedCareerPathDigest> items, List<string> order) : ICappedCareerPathDigestStore
{
    public Dictionary<string, CappedCareerPathDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedCareerPathDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedCareerPathDigest item)
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

    public List<CappedCareerPathDigest> List()
    {
        var outValue = new List<CappedCareerPathDigest>();
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
public sealed class CachedCappedCareerPathDigestStore(MemoryCappedCareerPathDigestStore inner, CappedCareerPathDigestCache cache, int hits, int misses) : ICappedCareerPathDigestStore
{
    public MemoryCappedCareerPathDigestStore Inner { get; set; } = inner;
    public CappedCareerPathDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedCareerPathDigest? Find(string code)
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

    public void Save(CappedCareerPathDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedCareerPathDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped career path digests. Empty fields match all.</summary>
public sealed record CappedCareerPathDigestQuery(string Source, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(CappedCareerPathDigest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<CappedCareerPathDigest> Apply(List<CappedCareerPathDigest> items)
    {
        var matched = Collections.Filter(items, (CappedCareerPathDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped career path digest store.</summary>
public static class CappedCareerPathDigestStore
{
    public static ICappedCareerPathDigestStore NewMemoryCappedCareerPathDigestStore()
    {
        return new MemoryCappedCareerPathDigestStore(
            new Dictionary<string, CappedCareerPathDigest>(),
            new List<string>());
    }

    public static ICappedCareerPathDigestStore NewCachedCappedCareerPathDigestStore(MemoryCappedCareerPathDigestStore inner, int ttl, IClock clock)
    {
        return new CachedCappedCareerPathDigestStore(
            inner,
            CappedCareerPathDigestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedCareerPathDigest> FindCappedCareerPathDigests(ICappedCareerPathDigestStore store, CappedCareerPathDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
