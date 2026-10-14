namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment assignments by code.</summary>
public interface IAdjustmentAssignmentStore
{
    AdjustmentAssignment? Find(string code);
    void Save(AdjustmentAssignment item);
    bool Remove(string code);
    List<AdjustmentAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentAssignmentStore(Dictionary<string, AdjustmentAssignment> items, List<string> order) : IAdjustmentAssignmentStore
{
    public Dictionary<string, AdjustmentAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentAssignment item)
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

    public List<AdjustmentAssignment> List()
    {
        var outValue = new List<AdjustmentAssignment>();
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
public sealed class CachedAdjustmentAssignmentStore(MemoryAdjustmentAssignmentStore inner, AdjustmentAssignmentCache cache, int hits, int misses) : IAdjustmentAssignmentStore
{
    public MemoryAdjustmentAssignmentStore Inner { get; set; } = inner;
    public AdjustmentAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentAssignment? Find(string code)
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

    public void Save(AdjustmentAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment assignments. Empty fields match all.</summary>
public sealed record AdjustmentAssignmentQuery(string Country, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentAssignment item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentAssignment> Apply(List<AdjustmentAssignment> items)
    {
        var matched = Collections.Filter(items, (AdjustmentAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment assignment store.</summary>
public static class AdjustmentAssignmentStore
{
    public static IAdjustmentAssignmentStore NewMemoryAdjustmentAssignmentStore()
    {
        return new MemoryAdjustmentAssignmentStore(
            new Dictionary<string, AdjustmentAssignment>(),
            new List<string>());
    }

    public static IAdjustmentAssignmentStore NewCachedAdjustmentAssignmentStore(MemoryAdjustmentAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentAssignmentStore(inner, AdjustmentAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AdjustmentAssignment> FindAdjustmentAssignments(IAdjustmentAssignmentStore store, AdjustmentAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
