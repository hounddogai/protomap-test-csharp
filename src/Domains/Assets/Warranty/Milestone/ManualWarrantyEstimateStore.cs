namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual warranty estimates by code.</summary>
public interface IManualWarrantyEstimateStore
{
    ManualWarrantyEstimate? Find(string code);
    void Save(ManualWarrantyEstimate item);
    bool Remove(string code);
    List<ManualWarrantyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualWarrantyEstimateStore(Dictionary<string, ManualWarrantyEstimate> items, List<string> order) : IManualWarrantyEstimateStore
{
    public Dictionary<string, ManualWarrantyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualWarrantyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualWarrantyEstimate item)
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

    public List<ManualWarrantyEstimate> List()
    {
        var outValue = new List<ManualWarrantyEstimate>();
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
public sealed class CachedManualWarrantyEstimateStore(MemoryManualWarrantyEstimateStore inner, ManualWarrantyEstimateCache cache, int hits, int misses) : IManualWarrantyEstimateStore
{
    public MemoryManualWarrantyEstimateStore Inner { get; set; } = inner;
    public ManualWarrantyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualWarrantyEstimate? Find(string code)
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

    public void Save(ManualWarrantyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualWarrantyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual warranty estimates. Empty fields match all.</summary>
public sealed record ManualWarrantyEstimateQuery(string Locale, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualWarrantyEstimate item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualWarrantyEstimate> Apply(List<ManualWarrantyEstimate> items)
    {
        var matched = Collections.Filter(items, (ManualWarrantyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual warranty estimate store.</summary>
public static class ManualWarrantyEstimateStore
{
    public static IManualWarrantyEstimateStore NewMemoryManualWarrantyEstimateStore()
    {
        return new MemoryManualWarrantyEstimateStore(
            new Dictionary<string, ManualWarrantyEstimate>(),
            new List<string>());
    }

    public static IManualWarrantyEstimateStore NewCachedManualWarrantyEstimateStore(MemoryManualWarrantyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedManualWarrantyEstimateStore(
            inner,
            ManualWarrantyEstimateCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualWarrantyEstimate> FindManualWarrantyEstimates(IManualWarrantyEstimateStore store, ManualWarrantyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
