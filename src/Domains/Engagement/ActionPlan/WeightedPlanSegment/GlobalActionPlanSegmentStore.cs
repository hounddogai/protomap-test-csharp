namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global action plan segments by code.</summary>
public interface IGlobalActionPlanSegmentStore
{
    GlobalActionPlanSegment? Find(string code);
    void Save(GlobalActionPlanSegment item);
    bool Remove(string code);
    List<GlobalActionPlanSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalActionPlanSegmentStore(Dictionary<string, GlobalActionPlanSegment> items, List<string> order) : IGlobalActionPlanSegmentStore
{
    public Dictionary<string, GlobalActionPlanSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalActionPlanSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalActionPlanSegment item)
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

    public List<GlobalActionPlanSegment> List()
    {
        var outValue = new List<GlobalActionPlanSegment>();
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
public sealed class CachedGlobalActionPlanSegmentStore(MemoryGlobalActionPlanSegmentStore inner, GlobalActionPlanSegmentCache cache, int hits, int misses) : IGlobalActionPlanSegmentStore
{
    public MemoryGlobalActionPlanSegmentStore Inner { get; set; } = inner;
    public GlobalActionPlanSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalActionPlanSegment? Find(string code)
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

    public void Save(GlobalActionPlanSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalActionPlanSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global action plan segments. Empty fields match all.</summary>
public sealed record GlobalActionPlanSegmentQuery(string Name, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalActionPlanSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalActionPlanSegment> Apply(List<GlobalActionPlanSegment> items)
    {
        var matched = Collections.Filter(items, (GlobalActionPlanSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global action plan segment store.</summary>
public static class GlobalActionPlanSegmentStore
{
    public static IGlobalActionPlanSegmentStore NewMemoryGlobalActionPlanSegmentStore()
    {
        return new MemoryGlobalActionPlanSegmentStore(
            new Dictionary<string, GlobalActionPlanSegment>(),
            new List<string>());
    }

    public static IGlobalActionPlanSegmentStore NewCachedGlobalActionPlanSegmentStore(MemoryGlobalActionPlanSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalActionPlanSegmentStore(
            inner,
            GlobalActionPlanSegmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalActionPlanSegment> FindGlobalActionPlanSegments(IGlobalActionPlanSegmentStore store, GlobalActionPlanSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
