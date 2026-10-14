namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary rate requests by code.</summary>
public interface IPrimaryRateRequestStore
{
    PrimaryRateRequest? Find(string code);
    void Save(PrimaryRateRequest item);
    bool Remove(string code);
    List<PrimaryRateRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryRateRequestStore(Dictionary<string, PrimaryRateRequest> items, List<string> order) : IPrimaryRateRequestStore
{
    public Dictionary<string, PrimaryRateRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryRateRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryRateRequest item)
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

    public List<PrimaryRateRequest> List()
    {
        var outValue = new List<PrimaryRateRequest>();
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
public sealed class CachedPrimaryRateRequestStore(MemoryPrimaryRateRequestStore inner, PrimaryRateRequestCache cache, int hits, int misses) : IPrimaryRateRequestStore
{
    public MemoryPrimaryRateRequestStore Inner { get; set; } = inner;
    public PrimaryRateRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryRateRequest? Find(string code)
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

    public void Save(PrimaryRateRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryRateRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary rate requests. Empty fields match all.</summary>
public sealed record PrimaryRateRequestQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryRateRequest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryRateRequest> Apply(List<PrimaryRateRequest> items)
    {
        var matched = Collections.Filter(items, (PrimaryRateRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary rate request store.</summary>
public static class PrimaryRateRequestStore
{
    public static IPrimaryRateRequestStore NewMemoryPrimaryRateRequestStore()
    {
        return new MemoryPrimaryRateRequestStore(new Dictionary<string, PrimaryRateRequest>(), new List<string>());
    }

    public static IPrimaryRateRequestStore NewCachedPrimaryRateRequestStore(MemoryPrimaryRateRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryRateRequestStore(inner, PrimaryRateRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PrimaryRateRequest> FindPrimaryRateRequests(IPrimaryRateRequestStore store, PrimaryRateRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
