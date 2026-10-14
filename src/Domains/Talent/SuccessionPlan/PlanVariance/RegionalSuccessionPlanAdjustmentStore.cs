namespace Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional succession plan adjustments by code.</summary>
public interface IRegionalSuccessionPlanAdjustmentStore
{
    RegionalSuccessionPlanAdjustment? Find(string code);
    void Save(RegionalSuccessionPlanAdjustment item);
    bool Remove(string code);
    List<RegionalSuccessionPlanAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalSuccessionPlanAdjustmentStore(Dictionary<string, RegionalSuccessionPlanAdjustment> items, List<string> order) : IRegionalSuccessionPlanAdjustmentStore
{
    public Dictionary<string, RegionalSuccessionPlanAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalSuccessionPlanAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalSuccessionPlanAdjustment item)
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

    public List<RegionalSuccessionPlanAdjustment> List()
    {
        var outValue = new List<RegionalSuccessionPlanAdjustment>();
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
public sealed class CachedRegionalSuccessionPlanAdjustmentStore(MemoryRegionalSuccessionPlanAdjustmentStore inner, RegionalSuccessionPlanAdjustmentCache cache, int hits, int misses) : IRegionalSuccessionPlanAdjustmentStore
{
    public MemoryRegionalSuccessionPlanAdjustmentStore Inner { get; set; } = inner;
    public RegionalSuccessionPlanAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalSuccessionPlanAdjustment? Find(string code)
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

    public void Save(RegionalSuccessionPlanAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalSuccessionPlanAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional succession plan adjustments. Empty fields match all.</summary>
public sealed record RegionalSuccessionPlanAdjustmentQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalSuccessionPlanAdjustment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalSuccessionPlanAdjustment> Apply(List<RegionalSuccessionPlanAdjustment> items)
    {
        var matched = Collections.Filter(items, (RegionalSuccessionPlanAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional succession plan adjustment store.</summary>
public static class RegionalSuccessionPlanAdjustmentStore
{
    public static IRegionalSuccessionPlanAdjustmentStore NewMemoryRegionalSuccessionPlanAdjustmentStore()
    {
        return new MemoryRegionalSuccessionPlanAdjustmentStore(
            new Dictionary<string, RegionalSuccessionPlanAdjustment>(),
            new List<string>());
    }

    public static IRegionalSuccessionPlanAdjustmentStore NewCachedRegionalSuccessionPlanAdjustmentStore(MemoryRegionalSuccessionPlanAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalSuccessionPlanAdjustmentStore(
            inner,
            RegionalSuccessionPlanAdjustmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<RegionalSuccessionPlanAdjustment> FindRegionalSuccessionPlanAdjustments(IRegionalSuccessionPlanAdjustmentStore store, RegionalSuccessionPlanAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
