namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending floor plan targets by code.</summary>
public interface IPendingFloorPlanTargetStore
{
    PendingFloorPlanTarget? Find(string code);
    void Save(PendingFloorPlanTarget item);
    bool Remove(string code);
    List<PendingFloorPlanTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingFloorPlanTargetStore(Dictionary<string, PendingFloorPlanTarget> items, List<string> order) : IPendingFloorPlanTargetStore
{
    public Dictionary<string, PendingFloorPlanTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingFloorPlanTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingFloorPlanTarget item)
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

    public List<PendingFloorPlanTarget> List()
    {
        var outValue = new List<PendingFloorPlanTarget>();
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
public sealed class CachedPendingFloorPlanTargetStore(MemoryPendingFloorPlanTargetStore inner, PendingFloorPlanTargetCache cache, int hits, int misses) : IPendingFloorPlanTargetStore
{
    public MemoryPendingFloorPlanTargetStore Inner { get; set; } = inner;
    public PendingFloorPlanTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingFloorPlanTarget? Find(string code)
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

    public void Save(PendingFloorPlanTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingFloorPlanTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending floor plan targets. Empty fields match all.</summary>
public sealed record PendingFloorPlanTargetQuery(string Label, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingFloorPlanTarget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PendingFloorPlanTarget> Apply(List<PendingFloorPlanTarget> items)
    {
        var matched = Collections.Filter(items, (PendingFloorPlanTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending floor plan target store.</summary>
public static class PendingFloorPlanTargetStore
{
    public static IPendingFloorPlanTargetStore NewMemoryPendingFloorPlanTargetStore()
    {
        return new MemoryPendingFloorPlanTargetStore(
            new Dictionary<string, PendingFloorPlanTarget>(),
            new List<string>());
    }

    public static IPendingFloorPlanTargetStore NewCachedPendingFloorPlanTargetStore(MemoryPendingFloorPlanTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPendingFloorPlanTargetStore(
            inner,
            PendingFloorPlanTargetCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingFloorPlanTarget> FindPendingFloorPlanTargets(IPendingFloorPlanTargetStore store, PendingFloorPlanTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
