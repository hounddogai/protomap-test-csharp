namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps floor plan drafts by code.</summary>
public interface IFloorPlanDraftStore
{
    FloorPlanDraft? Find(string code);
    void Save(FloorPlanDraft item);
    bool Remove(string code);
    List<FloorPlanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFloorPlanDraftStore(Dictionary<string, FloorPlanDraft> items, List<string> order) : IFloorPlanDraftStore
{
    public Dictionary<string, FloorPlanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FloorPlanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FloorPlanDraft item)
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

    public List<FloorPlanDraft> List()
    {
        var outValue = new List<FloorPlanDraft>();
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
public sealed class CachedFloorPlanDraftStore(MemoryFloorPlanDraftStore inner, FloorPlanDraftCache cache, int hits, int misses) : IFloorPlanDraftStore
{
    public MemoryFloorPlanDraftStore Inner { get; set; } = inner;
    public FloorPlanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FloorPlanDraft? Find(string code)
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

    public void Save(FloorPlanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FloorPlanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects floor plan drafts. Empty fields match all.</summary>
public sealed record FloorPlanDraftQuery(string Team, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(FloorPlanDraft item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FloorPlanDraft> Apply(List<FloorPlanDraft> items)
    {
        var matched = Collections.Filter(items, (FloorPlanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of floor plan draft store.</summary>
public static class FloorPlanDraftStore
{
    public static IFloorPlanDraftStore NewMemoryFloorPlanDraftStore()
    {
        return new MemoryFloorPlanDraftStore(new Dictionary<string, FloorPlanDraft>(), new List<string>());
    }

    public static IFloorPlanDraftStore NewCachedFloorPlanDraftStore(MemoryFloorPlanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedFloorPlanDraftStore(inner, FloorPlanDraftCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FloorPlanDraft> FindFloorPlanDrafts(IFloorPlanDraftStore store, FloorPlanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
