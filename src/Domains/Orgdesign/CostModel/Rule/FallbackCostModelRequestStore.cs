namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback cost model requests by code.</summary>
public interface IFallbackCostModelRequestStore
{
    FallbackCostModelRequest? Find(string code);
    void Save(FallbackCostModelRequest item);
    bool Remove(string code);
    List<FallbackCostModelRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackCostModelRequestStore(Dictionary<string, FallbackCostModelRequest> items, List<string> order) : IFallbackCostModelRequestStore
{
    public Dictionary<string, FallbackCostModelRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackCostModelRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackCostModelRequest item)
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

    public List<FallbackCostModelRequest> List()
    {
        var outValue = new List<FallbackCostModelRequest>();
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
public sealed class CachedFallbackCostModelRequestStore(MemoryFallbackCostModelRequestStore inner, FallbackCostModelRequestCache cache, int hits, int misses) : IFallbackCostModelRequestStore
{
    public MemoryFallbackCostModelRequestStore Inner { get; set; } = inner;
    public FallbackCostModelRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackCostModelRequest? Find(string code)
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

    public void Save(FallbackCostModelRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackCostModelRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback cost model requests. Empty fields match all.</summary>
public sealed record FallbackCostModelRequestQuery(string Segment, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackCostModelRequest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackCostModelRequest> Apply(List<FallbackCostModelRequest> items)
    {
        var matched = Collections.Filter(items, (FallbackCostModelRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback cost model request store.</summary>
public static class FallbackCostModelRequestStore
{
    public static IFallbackCostModelRequestStore NewMemoryFallbackCostModelRequestStore()
    {
        return new MemoryFallbackCostModelRequestStore(
            new Dictionary<string, FallbackCostModelRequest>(),
            new List<string>());
    }

    public static IFallbackCostModelRequestStore NewCachedFallbackCostModelRequestStore(MemoryFallbackCostModelRequestStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackCostModelRequestStore(
            inner,
            FallbackCostModelRequestCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackCostModelRequest> FindFallbackCostModelRequests(IFallbackCostModelRequestStore store, FallbackCostModelRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
