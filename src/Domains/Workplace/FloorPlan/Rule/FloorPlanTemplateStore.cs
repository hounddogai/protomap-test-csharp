namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan templates by code.</summary>
public interface IFloorPlanTemplateStore
{
    FloorPlanTemplate? Find(string code);
    void Save(FloorPlanTemplate item);
    bool Remove(string code);
    List<FloorPlanTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanTemplateStore(Dictionary<string, FloorPlanTemplate> items, List<string> order) : IFloorPlanTemplateStore
{
    public Dictionary<string, FloorPlanTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanTemplate item)
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

    public List<FloorPlanTemplate> List()
    {
        var outValue = new List<FloorPlanTemplate>();
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
public sealed class CachedFloorPlanTemplateStore(MemoryFloorPlanTemplateStore inner, FloorPlanTemplateCache cache, int hits, int misses) : IFloorPlanTemplateStore
{
    public MemoryFloorPlanTemplateStore Inner { get; set; } = inner;
    public FloorPlanTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanTemplate? Find(string code)
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

    public void Save(FloorPlanTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan templates. Empty fields match all.</summary>
public sealed record FloorPlanTemplateQuery(string Label, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanTemplate> Apply(List<FloorPlanTemplate> items)
    {
        var matched = Collections.Filter(items, (FloorPlanTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan template store.</summary>
public static class FloorPlanTemplateStore
{
    public static IFloorPlanTemplateStore NewMemoryFloorPlanTemplateStore()
    {
        return new MemoryFloorPlanTemplateStore(new Dictionary<string, FloorPlanTemplate>(), new List<string>());
    }

    public static IFloorPlanTemplateStore NewCachedFloorPlanTemplateStore(MemoryFloorPlanTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanTemplateStore(inner, FloorPlanTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FloorPlanTemplate> FindFloorPlanTemplates(IFloorPlanTemplateStore store, FloorPlanTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
