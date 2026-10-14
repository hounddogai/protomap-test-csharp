namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered pay band allocations by code.</summary>
public interface ITieredPayBandAllocationStore
{
    TieredPayBandAllocation? Find(string code);
    void Save(TieredPayBandAllocation item);
    bool Remove(string code);
    List<TieredPayBandAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredPayBandAllocationStore(Dictionary<string, TieredPayBandAllocation> items, List<string> order) : ITieredPayBandAllocationStore
{
    public Dictionary<string, TieredPayBandAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredPayBandAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredPayBandAllocation item)
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

    public List<TieredPayBandAllocation> List()
    {
        var outValue = new List<TieredPayBandAllocation>();
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
public sealed class CachedTieredPayBandAllocationStore(MemoryTieredPayBandAllocationStore inner, TieredPayBandAllocationCache cache, int hits, int misses) : ITieredPayBandAllocationStore
{
    public MemoryTieredPayBandAllocationStore Inner { get; set; } = inner;
    public TieredPayBandAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredPayBandAllocation? Find(string code)
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

    public void Save(TieredPayBandAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredPayBandAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered pay band allocations. Empty fields match all.</summary>
public sealed record TieredPayBandAllocationQuery(string Segment, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(TieredPayBandAllocation item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredPayBandAllocation> Apply(List<TieredPayBandAllocation> items)
    {
        var matched = Collections.Filter(items, (TieredPayBandAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered pay band allocation store.</summary>
public static class TieredPayBandAllocationStore
{
    public static ITieredPayBandAllocationStore NewMemoryTieredPayBandAllocationStore()
    {
        return new MemoryTieredPayBandAllocationStore(
            new Dictionary<string, TieredPayBandAllocation>(),
            new List<string>());
    }

    public static ITieredPayBandAllocationStore NewCachedTieredPayBandAllocationStore(MemoryTieredPayBandAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedTieredPayBandAllocationStore(
            inner,
            TieredPayBandAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredPayBandAllocation> FindTieredPayBandAllocations(ITieredPayBandAllocationStore store, TieredPayBandAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
