namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional balance digests by code.</summary>
public interface IRegionalBalanceDigestStore
{
    RegionalBalanceDigest? Find(string code);
    void Save(RegionalBalanceDigest item);
    bool Remove(string code);
    List<RegionalBalanceDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalBalanceDigestStore(Dictionary<string, RegionalBalanceDigest> items, List<string> order) : IRegionalBalanceDigestStore
{
    public Dictionary<string, RegionalBalanceDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalBalanceDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalBalanceDigest item)
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

    public List<RegionalBalanceDigest> List()
    {
        var outValue = new List<RegionalBalanceDigest>();
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
public sealed class CachedRegionalBalanceDigestStore(MemoryRegionalBalanceDigestStore inner, RegionalBalanceDigestCache cache, int hits, int misses) : IRegionalBalanceDigestStore
{
    public MemoryRegionalBalanceDigestStore Inner { get; set; } = inner;
    public RegionalBalanceDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalBalanceDigest? Find(string code)
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

    public void Save(RegionalBalanceDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalBalanceDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional balance digests. Empty fields match all.</summary>
public sealed record RegionalBalanceDigestQuery(string Team, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalBalanceDigest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalBalanceDigest> Apply(List<RegionalBalanceDigest> items)
    {
        var matched = Collections.Filter(items, (RegionalBalanceDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional balance digest store.</summary>
public static class RegionalBalanceDigestStore
{
    public static IRegionalBalanceDigestStore NewMemoryRegionalBalanceDigestStore()
    {
        return new MemoryRegionalBalanceDigestStore(
            new Dictionary<string, RegionalBalanceDigest>(),
            new List<string>());
    }

    public static IRegionalBalanceDigestStore NewCachedRegionalBalanceDigestStore(MemoryRegionalBalanceDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalBalanceDigestStore(inner, RegionalBalanceDigestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RegionalBalanceDigest> FindRegionalBalanceDigests(IRegionalBalanceDigestStore store, RegionalBalanceDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
