namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate requests by code.</summary>
public interface IRateRequestStore
{
    RateRequest? Find(string code);
    void Save(RateRequest item);
    bool Remove(string code);
    List<RateRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateRequestStore(Dictionary<string, RateRequest> items, List<string> order) : IRateRequestStore
{
    public Dictionary<string, RateRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateRequest item)
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

    public List<RateRequest> List()
    {
        var outValue = new List<RateRequest>();
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
public sealed class CachedRateRequestStore(MemoryRateRequestStore inner, RateRequestCache cache, int hits, int misses) : IRateRequestStore
{
    public MemoryRateRequestStore Inner { get; set; } = inner;
    public RateRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateRequest? Find(string code)
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

    public void Save(RateRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate requests. Empty fields match all.</summary>
public sealed record RateRequestQuery(string Source, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(RateRequest item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<RateRequest> Apply(List<RateRequest> items)
    {
        var matched = Collections.Filter(items, (RateRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate request store.</summary>
public static class RateRequestStore
{
    public static IRateRequestStore NewMemoryRateRequestStore()
    {
        return new MemoryRateRequestStore(new Dictionary<string, RateRequest>(), new List<string>());
    }

    public static IRateRequestStore NewCachedRateRequestStore(MemoryRateRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRateRequestStore(inner, RateRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RateRequest> FindRateRequests(IRateRequestStore store, RateRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
