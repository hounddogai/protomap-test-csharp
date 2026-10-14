namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal warranty digests by code.</summary>
public interface ISeasonalWarrantyDigestStore
{
    SeasonalWarrantyDigest? Find(string code);
    void Save(SeasonalWarrantyDigest item);
    bool Remove(string code);
    List<SeasonalWarrantyDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalWarrantyDigestStore(Dictionary<string, SeasonalWarrantyDigest> items, List<string> order) : ISeasonalWarrantyDigestStore
{
    public Dictionary<string, SeasonalWarrantyDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalWarrantyDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalWarrantyDigest item)
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

    public List<SeasonalWarrantyDigest> List()
    {
        var outValue = new List<SeasonalWarrantyDigest>();
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
public sealed class CachedSeasonalWarrantyDigestStore(MemorySeasonalWarrantyDigestStore inner, SeasonalWarrantyDigestCache cache, int hits, int misses) : ISeasonalWarrantyDigestStore
{
    public MemorySeasonalWarrantyDigestStore Inner { get; set; } = inner;
    public SeasonalWarrantyDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalWarrantyDigest? Find(string code)
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

    public void Save(SeasonalWarrantyDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalWarrantyDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal warranty digests. Empty fields match all.</summary>
public sealed record SeasonalWarrantyDigestQuery(string Team, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalWarrantyDigest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalWarrantyDigest> Apply(List<SeasonalWarrantyDigest> items)
    {
        var matched = Collections.Filter(items, (SeasonalWarrantyDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal warranty digest store.</summary>
public static class SeasonalWarrantyDigestStore
{
    public static ISeasonalWarrantyDigestStore NewMemorySeasonalWarrantyDigestStore()
    {
        return new MemorySeasonalWarrantyDigestStore(
            new Dictionary<string, SeasonalWarrantyDigest>(),
            new List<string>());
    }

    public static ISeasonalWarrantyDigestStore NewCachedSeasonalWarrantyDigestStore(MemorySeasonalWarrantyDigestStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalWarrantyDigestStore(
            inner,
            SeasonalWarrantyDigestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalWarrantyDigest> FindSeasonalWarrantyDigests(ISeasonalWarrantyDigestStore store, SeasonalWarrantyDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
