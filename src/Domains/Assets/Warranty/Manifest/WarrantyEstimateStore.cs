namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps warranty estimates by code.</summary>
public interface IWarrantyEstimateStore
{
    WarrantyEstimate? Find(string code);
    void Save(WarrantyEstimate item);
    bool Remove(string code);
    List<WarrantyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWarrantyEstimateStore(Dictionary<string, WarrantyEstimate> items, List<string> order) : IWarrantyEstimateStore
{
    public Dictionary<string, WarrantyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WarrantyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WarrantyEstimate item)
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

    public List<WarrantyEstimate> List()
    {
        var outValue = new List<WarrantyEstimate>();
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
public sealed class CachedWarrantyEstimateStore(MemoryWarrantyEstimateStore inner, WarrantyEstimateCache cache, int hits, int misses) : IWarrantyEstimateStore
{
    public MemoryWarrantyEstimateStore Inner { get; set; } = inner;
    public WarrantyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WarrantyEstimate? Find(string code)
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

    public void Save(WarrantyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WarrantyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects warranty estimates. Empty fields match all.</summary>
public sealed record WarrantyEstimateQuery(string Region, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(WarrantyEstimate item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WarrantyEstimate> Apply(List<WarrantyEstimate> items)
    {
        var matched = Collections.Filter(items, (WarrantyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of warranty estimate store.</summary>
public static class WarrantyEstimateStore
{
    public static IWarrantyEstimateStore NewMemoryWarrantyEstimateStore()
    {
        return new MemoryWarrantyEstimateStore(new Dictionary<string, WarrantyEstimate>(), new List<string>());
    }

    public static IWarrantyEstimateStore NewCachedWarrantyEstimateStore(MemoryWarrantyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedWarrantyEstimateStore(inner, WarrantyEstimateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WarrantyEstimate> FindWarrantyEstimates(IWarrantyEstimateStore store, WarrantyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
