namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict floor plan drafts by code.</summary>
public interface IStrictFloorPlanDraftStore
{
    StrictFloorPlanDraft? Find(string code);
    void Save(StrictFloorPlanDraft item);
    bool Remove(string code);
    List<StrictFloorPlanDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictFloorPlanDraftStore(Dictionary<string, StrictFloorPlanDraft> items, List<string> order) : IStrictFloorPlanDraftStore
{
    public Dictionary<string, StrictFloorPlanDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictFloorPlanDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictFloorPlanDraft item)
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

    public List<StrictFloorPlanDraft> List()
    {
        var outValue = new List<StrictFloorPlanDraft>();
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
public sealed class CachedStrictFloorPlanDraftStore(MemoryStrictFloorPlanDraftStore inner, StrictFloorPlanDraftCache cache, int hits, int misses) : IStrictFloorPlanDraftStore
{
    public MemoryStrictFloorPlanDraftStore Inner { get; set; } = inner;
    public StrictFloorPlanDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictFloorPlanDraft? Find(string code)
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

    public void Save(StrictFloorPlanDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictFloorPlanDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict floor plan drafts. Empty fields match all.</summary>
public sealed record StrictFloorPlanDraftQuery(string Team, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictFloorPlanDraft item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictFloorPlanDraft> Apply(List<StrictFloorPlanDraft> items)
    {
        var matched = Collections.Filter(items, (StrictFloorPlanDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict floor plan draft store.</summary>
public static class StrictFloorPlanDraftStore
{
    public static IStrictFloorPlanDraftStore NewMemoryStrictFloorPlanDraftStore()
    {
        return new MemoryStrictFloorPlanDraftStore(
            new Dictionary<string, StrictFloorPlanDraft>(),
            new List<string>());
    }

    public static IStrictFloorPlanDraftStore NewCachedStrictFloorPlanDraftStore(MemoryStrictFloorPlanDraftStore inner, int ttl, IClock clock)
    {
        return new CachedStrictFloorPlanDraftStore(inner, StrictFloorPlanDraftCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictFloorPlanDraft> FindStrictFloorPlanDrafts(IStrictFloorPlanDraftStore store, StrictFloorPlanDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
