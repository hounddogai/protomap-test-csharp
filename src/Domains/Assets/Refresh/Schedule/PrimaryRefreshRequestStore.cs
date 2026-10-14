namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary refresh requests by code.</summary>
public interface IPrimaryRefreshRequestStore
{
    PrimaryRefreshRequest? Find(string code);
    void Save(PrimaryRefreshRequest item);
    bool Remove(string code);
    List<PrimaryRefreshRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRefreshRequestStore(Dictionary<string, PrimaryRefreshRequest> items, List<string> order) : IPrimaryRefreshRequestStore
{
    public Dictionary<string, PrimaryRefreshRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRefreshRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRefreshRequest item)
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

    public List<PrimaryRefreshRequest> List()
    {
        var outValue = new List<PrimaryRefreshRequest>();
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
public sealed class CachedPrimaryRefreshRequestStore(MemoryPrimaryRefreshRequestStore inner, PrimaryRefreshRequestCache cache, int hits, int misses) : IPrimaryRefreshRequestStore
{
    public MemoryPrimaryRefreshRequestStore Inner { get; set; } = inner;
    public PrimaryRefreshRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRefreshRequest? Find(string code)
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

    public void Save(PrimaryRefreshRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRefreshRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary refresh requests. Empty fields match all.</summary>
public sealed record PrimaryRefreshRequestQuery(string Source, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRefreshRequest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryRefreshRequest> Apply(List<PrimaryRefreshRequest> items)
    {
        var matched = Collections.Filter(items, (PrimaryRefreshRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary refresh request store.</summary>
public static class PrimaryRefreshRequestStore
{
    public static IPrimaryRefreshRequestStore NewMemoryPrimaryRefreshRequestStore()
    {
        return new MemoryPrimaryRefreshRequestStore(
            new Dictionary<string, PrimaryRefreshRequest>(),
            new List<string>());
    }

    public static IPrimaryRefreshRequestStore NewCachedPrimaryRefreshRequestStore(MemoryPrimaryRefreshRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRefreshRequestStore(
            inner,
            PrimaryRefreshRequestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryRefreshRequest> FindPrimaryRefreshRequests(IPrimaryRefreshRequestStore store, PrimaryRefreshRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
