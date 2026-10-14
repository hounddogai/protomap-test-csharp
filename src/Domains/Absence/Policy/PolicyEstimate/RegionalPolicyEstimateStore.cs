namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional policy estimates by code.</summary>
public interface IRegionalPolicyEstimateStore
{
    RegionalPolicyEstimate? Find(string code);
    void Save(RegionalPolicyEstimate item);
    bool Remove(string code);
    List<RegionalPolicyEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalPolicyEstimateStore(Dictionary<string, RegionalPolicyEstimate> items, List<string> order) : IRegionalPolicyEstimateStore
{
    public Dictionary<string, RegionalPolicyEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalPolicyEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalPolicyEstimate item)
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

    public List<RegionalPolicyEstimate> List()
    {
        var outValue = new List<RegionalPolicyEstimate>();
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
public sealed class CachedRegionalPolicyEstimateStore(MemoryRegionalPolicyEstimateStore inner, RegionalPolicyEstimateCache cache, int hits, int misses) : IRegionalPolicyEstimateStore
{
    public MemoryRegionalPolicyEstimateStore Inner { get; set; } = inner;
    public RegionalPolicyEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalPolicyEstimate? Find(string code)
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

    public void Save(RegionalPolicyEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalPolicyEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional policy estimates. Empty fields match all.</summary>
public sealed record RegionalPolicyEstimateQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalPolicyEstimate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalPolicyEstimate> Apply(List<RegionalPolicyEstimate> items)
    {
        var matched = Collections.Filter(items, (RegionalPolicyEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional policy estimate store.</summary>
public static class RegionalPolicyEstimateStore
{
    public static IRegionalPolicyEstimateStore NewMemoryRegionalPolicyEstimateStore()
    {
        return new MemoryRegionalPolicyEstimateStore(
            new Dictionary<string, RegionalPolicyEstimate>(),
            new List<string>());
    }

    public static IRegionalPolicyEstimateStore NewCachedRegionalPolicyEstimateStore(MemoryRegionalPolicyEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalPolicyEstimateStore(
            inner,
            RegionalPolicyEstimateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalPolicyEstimate> FindRegionalPolicyEstimates(IRegionalPolicyEstimateStore store, RegionalPolicyEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
