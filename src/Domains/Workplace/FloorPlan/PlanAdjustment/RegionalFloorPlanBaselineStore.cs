namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional floor plan baselines by code.</summary>
public interface IRegionalFloorPlanBaselineStore
{
    RegionalFloorPlanBaseline? Find(string code);
    void Save(RegionalFloorPlanBaseline item);
    bool Remove(string code);
    List<RegionalFloorPlanBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalFloorPlanBaselineStore(Dictionary<string, RegionalFloorPlanBaseline> items, List<string> order) : IRegionalFloorPlanBaselineStore
{
    public Dictionary<string, RegionalFloorPlanBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalFloorPlanBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalFloorPlanBaseline item)
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

    public List<RegionalFloorPlanBaseline> List()
    {
        var outValue = new List<RegionalFloorPlanBaseline>();
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
public sealed class CachedRegionalFloorPlanBaselineStore(MemoryRegionalFloorPlanBaselineStore inner, RegionalFloorPlanBaselineCache cache, int hits, int misses) : IRegionalFloorPlanBaselineStore
{
    public MemoryRegionalFloorPlanBaselineStore Inner { get; set; } = inner;
    public RegionalFloorPlanBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalFloorPlanBaseline? Find(string code)
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

    public void Save(RegionalFloorPlanBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalFloorPlanBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional floor plan baselines. Empty fields match all.</summary>
public sealed record RegionalFloorPlanBaselineQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalFloorPlanBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<RegionalFloorPlanBaseline> Apply(List<RegionalFloorPlanBaseline> items)
    {
        var matched = Collections.Filter(items, (RegionalFloorPlanBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional floor plan baseline store.</summary>
public static class RegionalFloorPlanBaselineStore
{
    public static IRegionalFloorPlanBaselineStore NewMemoryRegionalFloorPlanBaselineStore()
    {
        return new MemoryRegionalFloorPlanBaselineStore(
            new Dictionary<string, RegionalFloorPlanBaseline>(),
            new List<string>());
    }

    public static IRegionalFloorPlanBaselineStore NewCachedRegionalFloorPlanBaselineStore(MemoryRegionalFloorPlanBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalFloorPlanBaselineStore(
            inner,
            RegionalFloorPlanBaselineCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalFloorPlanBaseline> FindRegionalFloorPlanBaselines(IRegionalFloorPlanBaselineStore store, RegionalFloorPlanBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
