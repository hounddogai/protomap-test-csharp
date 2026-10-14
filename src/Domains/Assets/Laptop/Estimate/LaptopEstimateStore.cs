namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop estimates by code.</summary>
public interface ILaptopEstimateStore
{
    LaptopEstimate? Find(string code);
    void Save(LaptopEstimate item);
    bool Remove(string code);
    List<LaptopEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopEstimateStore(Dictionary<string, LaptopEstimate> items, List<string> order) : ILaptopEstimateStore
{
    public Dictionary<string, LaptopEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopEstimate item)
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

    public List<LaptopEstimate> List()
    {
        var outValue = new List<LaptopEstimate>();
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
public sealed class CachedLaptopEstimateStore(MemoryLaptopEstimateStore inner, LaptopEstimateCache cache, int hits, int misses) : ILaptopEstimateStore
{
    public MemoryLaptopEstimateStore Inner { get; set; } = inner;
    public LaptopEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopEstimate? Find(string code)
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

    public void Save(LaptopEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop estimates. Empty fields match all.</summary>
public sealed record LaptopEstimateQuery(string Segment, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopEstimate item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopEstimate> Apply(List<LaptopEstimate> items)
    {
        var matched = Collections.Filter(items, (LaptopEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop estimate store.</summary>
public static class LaptopEstimateStore
{
    public static ILaptopEstimateStore NewMemoryLaptopEstimateStore()
    {
        return new MemoryLaptopEstimateStore(new Dictionary<string, LaptopEstimate>(), new List<string>());
    }

    public static ILaptopEstimateStore NewCachedLaptopEstimateStore(MemoryLaptopEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopEstimateStore(inner, LaptopEstimateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LaptopEstimate> FindLaptopEstimates(ILaptopEstimateStore store, LaptopEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
