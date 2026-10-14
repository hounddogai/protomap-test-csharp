namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient accrual assignments by code.</summary>
public interface ILenientAccrualAssignmentStore
{
    LenientAccrualAssignment? Find(string code);
    void Save(LenientAccrualAssignment item);
    bool Remove(string code);
    List<LenientAccrualAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAccrualAssignmentStore(Dictionary<string, LenientAccrualAssignment> items, List<string> order) : ILenientAccrualAssignmentStore
{
    public Dictionary<string, LenientAccrualAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAccrualAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAccrualAssignment item)
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

    public List<LenientAccrualAssignment> List()
    {
        var outValue = new List<LenientAccrualAssignment>();
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
public sealed class CachedLenientAccrualAssignmentStore(MemoryLenientAccrualAssignmentStore inner, LenientAccrualAssignmentCache cache, int hits, int misses) : ILenientAccrualAssignmentStore
{
    public MemoryLenientAccrualAssignmentStore Inner { get; set; } = inner;
    public LenientAccrualAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAccrualAssignment? Find(string code)
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

    public void Save(LenientAccrualAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAccrualAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient accrual assignments. Empty fields match all.</summary>
public sealed record LenientAccrualAssignmentQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAccrualAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientAccrualAssignment> Apply(List<LenientAccrualAssignment> items)
    {
        var matched = Collections.Filter(items, (LenientAccrualAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient accrual assignment store.</summary>
public static class LenientAccrualAssignmentStore
{
    public static ILenientAccrualAssignmentStore NewMemoryLenientAccrualAssignmentStore()
    {
        return new MemoryLenientAccrualAssignmentStore(
            new Dictionary<string, LenientAccrualAssignment>(),
            new List<string>());
    }

    public static ILenientAccrualAssignmentStore NewCachedLenientAccrualAssignmentStore(MemoryLenientAccrualAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAccrualAssignmentStore(
            inner,
            LenientAccrualAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientAccrualAssignment> FindLenientAccrualAssignments(ILenientAccrualAssignmentStore store, LenientAccrualAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
