namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted floor plan entries by code.</summary>
public interface IWeightedFloorPlanEntryStore
{
    WeightedFloorPlanEntry? Find(string code);
    void Save(WeightedFloorPlanEntry item);
    bool Remove(string code);
    List<WeightedFloorPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedFloorPlanEntryStore(Dictionary<string, WeightedFloorPlanEntry> items, List<string> order) : IWeightedFloorPlanEntryStore
{
    public Dictionary<string, WeightedFloorPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedFloorPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedFloorPlanEntry item)
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

    public List<WeightedFloorPlanEntry> List()
    {
        var outValue = new List<WeightedFloorPlanEntry>();
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
public sealed class CachedWeightedFloorPlanEntryStore(MemoryWeightedFloorPlanEntryStore inner, WeightedFloorPlanEntryCache cache, int hits, int misses) : IWeightedFloorPlanEntryStore
{
    public MemoryWeightedFloorPlanEntryStore Inner { get; set; } = inner;
    public WeightedFloorPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedFloorPlanEntry? Find(string code)
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

    public void Save(WeightedFloorPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedFloorPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted floor plan entries. Empty fields match all.</summary>
public sealed record WeightedFloorPlanEntryQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedFloorPlanEntry item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedFloorPlanEntry> Apply(List<WeightedFloorPlanEntry> items)
    {
        var matched = Collections.Filter(items, (WeightedFloorPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted floor plan entry store.</summary>
public static class WeightedFloorPlanEntryStore
{
    public static IWeightedFloorPlanEntryStore NewMemoryWeightedFloorPlanEntryStore()
    {
        return new MemoryWeightedFloorPlanEntryStore(
            new Dictionary<string, WeightedFloorPlanEntry>(),
            new List<string>());
    }

    public static IWeightedFloorPlanEntryStore NewCachedWeightedFloorPlanEntryStore(MemoryWeightedFloorPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedFloorPlanEntryStore(
            inner,
            WeightedFloorPlanEntryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<WeightedFloorPlanEntry> FindWeightedFloorPlanEntries(IWeightedFloorPlanEntryStore store, WeightedFloorPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
