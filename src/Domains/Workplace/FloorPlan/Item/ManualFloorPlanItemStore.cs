namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual floor plan items by code.</summary>
public interface IManualFloorPlanItemStore
{
    ManualFloorPlanItem? Find(string code);
    void Save(ManualFloorPlanItem item);
    bool Remove(string code);
    List<ManualFloorPlanItem> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualFloorPlanItemStore(Dictionary<string, ManualFloorPlanItem> items, List<string> order) : IManualFloorPlanItemStore
{
    public Dictionary<string, ManualFloorPlanItem> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualFloorPlanItem? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualFloorPlanItem item)
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

    public List<ManualFloorPlanItem> List()
    {
        var outValue = new List<ManualFloorPlanItem>();
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
public sealed class CachedManualFloorPlanItemStore(MemoryManualFloorPlanItemStore inner, ManualFloorPlanItemCache cache, int hits, int misses) : IManualFloorPlanItemStore
{
    public MemoryManualFloorPlanItemStore Inner { get; set; } = inner;
    public ManualFloorPlanItemCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualFloorPlanItem? Find(string code)
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

    public void Save(ManualFloorPlanItem item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualFloorPlanItem> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual floor plan items. Empty fields match all.</summary>
public sealed record ManualFloorPlanItemQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(ManualFloorPlanItem item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<ManualFloorPlanItem> Apply(List<ManualFloorPlanItem> items)
    {
        var matched = Collections.Filter(items, (ManualFloorPlanItem candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual floor plan item store.</summary>
public static class ManualFloorPlanItemStore
{
    public static IManualFloorPlanItemStore NewMemoryManualFloorPlanItemStore()
    {
        return new MemoryManualFloorPlanItemStore(new Dictionary<string, ManualFloorPlanItem>(), new List<string>());
    }

    public static IManualFloorPlanItemStore NewCachedManualFloorPlanItemStore(MemoryManualFloorPlanItemStore inner, int ttl, IClock clock)
    {
        return new CachedManualFloorPlanItemStore(inner, ManualFloorPlanItemCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ManualFloorPlanItem> FindManualFloorPlanItems(IManualFloorPlanItemStore store, ManualFloorPlanItemQuery query)
    {
        return query.Apply(store.List());
    }
}
