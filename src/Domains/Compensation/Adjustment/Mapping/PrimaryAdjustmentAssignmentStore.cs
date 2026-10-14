namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary adjustment assignments by code.</summary>
public interface IPrimaryAdjustmentAssignmentStore
{
    PrimaryAdjustmentAssignment? Find(string code);
    void Save(PrimaryAdjustmentAssignment item);
    bool Remove(string code);
    List<PrimaryAdjustmentAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAdjustmentAssignmentStore(Dictionary<string, PrimaryAdjustmentAssignment> items, List<string> order) : IPrimaryAdjustmentAssignmentStore
{
    public Dictionary<string, PrimaryAdjustmentAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAdjustmentAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAdjustmentAssignment item)
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

    public List<PrimaryAdjustmentAssignment> List()
    {
        var outValue = new List<PrimaryAdjustmentAssignment>();
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
public sealed class CachedPrimaryAdjustmentAssignmentStore(MemoryPrimaryAdjustmentAssignmentStore inner, PrimaryAdjustmentAssignmentCache cache, int hits, int misses) : IPrimaryAdjustmentAssignmentStore
{
    public MemoryPrimaryAdjustmentAssignmentStore Inner { get; set; } = inner;
    public PrimaryAdjustmentAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAdjustmentAssignment? Find(string code)
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

    public void Save(PrimaryAdjustmentAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAdjustmentAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary adjustment assignments. Empty fields match all.</summary>
public sealed record PrimaryAdjustmentAssignmentQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAdjustmentAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAdjustmentAssignment> Apply(List<PrimaryAdjustmentAssignment> items)
    {
        var matched = Collections.Filter(items, (PrimaryAdjustmentAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary adjustment assignment store.</summary>
public static class PrimaryAdjustmentAssignmentStore
{
    public static IPrimaryAdjustmentAssignmentStore NewMemoryPrimaryAdjustmentAssignmentStore()
    {
        return new MemoryPrimaryAdjustmentAssignmentStore(
            new Dictionary<string, PrimaryAdjustmentAssignment>(),
            new List<string>());
    }

    public static IPrimaryAdjustmentAssignmentStore NewCachedPrimaryAdjustmentAssignmentStore(MemoryPrimaryAdjustmentAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAdjustmentAssignmentStore(
            inner,
            PrimaryAdjustmentAssignmentCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAdjustmentAssignment> FindPrimaryAdjustmentAssignments(IPrimaryAdjustmentAssignmentStore store, PrimaryAdjustmentAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
