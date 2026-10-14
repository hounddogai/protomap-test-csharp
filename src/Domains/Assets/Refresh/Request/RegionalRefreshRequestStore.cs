namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional refresh requests by code.</summary>
public interface IRegionalRefreshRequestStore
{
    RegionalRefreshRequest? Find(string code);
    void Save(RegionalRefreshRequest item);
    bool Remove(string code);
    List<RegionalRefreshRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalRefreshRequestStore(Dictionary<string, RegionalRefreshRequest> items, List<string> order) : IRegionalRefreshRequestStore
{
    public Dictionary<string, RegionalRefreshRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalRefreshRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalRefreshRequest item)
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

    public List<RegionalRefreshRequest> List()
    {
        var outValue = new List<RegionalRefreshRequest>();
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
public sealed class CachedRegionalRefreshRequestStore(MemoryRegionalRefreshRequestStore inner, RegionalRefreshRequestCache cache, int hits, int misses) : IRegionalRefreshRequestStore
{
    public MemoryRegionalRefreshRequestStore Inner { get; set; } = inner;
    public RegionalRefreshRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalRefreshRequest? Find(string code)
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

    public void Save(RegionalRefreshRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalRefreshRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional refresh requests. Empty fields match all.</summary>
public sealed record RegionalRefreshRequestQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalRefreshRequest item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalRefreshRequest> Apply(List<RegionalRefreshRequest> items)
    {
        var matched = Collections.Filter(items, (RegionalRefreshRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional refresh request store.</summary>
public static class RegionalRefreshRequestStore
{
    public static IRegionalRefreshRequestStore NewMemoryRegionalRefreshRequestStore()
    {
        return new MemoryRegionalRefreshRequestStore(
            new Dictionary<string, RegionalRefreshRequest>(),
            new List<string>());
    }

    public static IRegionalRefreshRequestStore NewCachedRegionalRefreshRequestStore(MemoryRegionalRefreshRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalRefreshRequestStore(
            inner,
            RegionalRefreshRequestCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalRefreshRequest> FindRegionalRefreshRequests(IRegionalRefreshRequestStore store, RegionalRefreshRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
