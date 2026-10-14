namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted rate requests by code.</summary>
public interface IWeightedRateRequestStore
{
    WeightedRateRequest? Find(string code);
    void Save(WeightedRateRequest item);
    bool Remove(string code);
    List<WeightedRateRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedRateRequestStore(Dictionary<string, WeightedRateRequest> items, List<string> order) : IWeightedRateRequestStore
{
    public Dictionary<string, WeightedRateRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedRateRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedRateRequest item)
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

    public List<WeightedRateRequest> List()
    {
        var outValue = new List<WeightedRateRequest>();
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
public sealed class CachedWeightedRateRequestStore(MemoryWeightedRateRequestStore inner, WeightedRateRequestCache cache, int hits, int misses) : IWeightedRateRequestStore
{
    public MemoryWeightedRateRequestStore Inner { get; set; } = inner;
    public WeightedRateRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedRateRequest? Find(string code)
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

    public void Save(WeightedRateRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedRateRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted rate requests. Empty fields match all.</summary>
public sealed record WeightedRateRequestQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedRateRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedRateRequest> Apply(List<WeightedRateRequest> items)
    {
        var matched = Collections.Filter(items, (WeightedRateRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted rate request store.</summary>
public static class WeightedRateRequestStore
{
    public static IWeightedRateRequestStore NewMemoryWeightedRateRequestStore()
    {
        return new MemoryWeightedRateRequestStore(new Dictionary<string, WeightedRateRequest>(), new List<string>());
    }

    public static IWeightedRateRequestStore NewCachedWeightedRateRequestStore(MemoryWeightedRateRequestStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedRateRequestStore(inner, WeightedRateRequestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<WeightedRateRequest> FindWeightedRateRequests(IWeightedRateRequestStore store, WeightedRateRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
