namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal access badge digests by code.</summary>
public interface ISeasonalAccessBadgeDigestStore
{
    SeasonalAccessBadgeDigest? Find(string code);
    void Save(SeasonalAccessBadgeDigest item);
    bool Remove(string code);
    List<SeasonalAccessBadgeDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAccessBadgeDigestStore(Dictionary<string, SeasonalAccessBadgeDigest> items, List<string> order) : ISeasonalAccessBadgeDigestStore
{
    public Dictionary<string, SeasonalAccessBadgeDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAccessBadgeDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAccessBadgeDigest item)
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

    public List<SeasonalAccessBadgeDigest> List()
    {
        var outValue = new List<SeasonalAccessBadgeDigest>();
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
public sealed class CachedSeasonalAccessBadgeDigestStore(MemorySeasonalAccessBadgeDigestStore inner, SeasonalAccessBadgeDigestCache cache, int hits, int misses) : ISeasonalAccessBadgeDigestStore
{
    public MemorySeasonalAccessBadgeDigestStore Inner { get; set; } = inner;
    public SeasonalAccessBadgeDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAccessBadgeDigest? Find(string code)
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

    public void Save(SeasonalAccessBadgeDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAccessBadgeDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal access badge digests. Empty fields match all.</summary>
public sealed record SeasonalAccessBadgeDigestQuery(string Country, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAccessBadgeDigest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAccessBadgeDigest> Apply(List<SeasonalAccessBadgeDigest> items)
    {
        var matched = Collections.Filter(items, (SeasonalAccessBadgeDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal access badge digest store.</summary>
public static class SeasonalAccessBadgeDigestStore
{
    public static ISeasonalAccessBadgeDigestStore NewMemorySeasonalAccessBadgeDigestStore()
    {
        return new MemorySeasonalAccessBadgeDigestStore(
            new Dictionary<string, SeasonalAccessBadgeDigest>(),
            new List<string>());
    }

    public static ISeasonalAccessBadgeDigestStore NewCachedSeasonalAccessBadgeDigestStore(MemorySeasonalAccessBadgeDigestStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAccessBadgeDigestStore(
            inner,
            SeasonalAccessBadgeDigestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAccessBadgeDigest> FindSeasonalAccessBadgeDigests(ISeasonalAccessBadgeDigestStore store, SeasonalAccessBadgeDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
