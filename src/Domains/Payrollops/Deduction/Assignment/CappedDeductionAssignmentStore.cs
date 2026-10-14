namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped deduction assignments by code.</summary>
public interface ICappedDeductionAssignmentStore
{
    CappedDeductionAssignment? Find(string code);
    void Save(CappedDeductionAssignment item);
    bool Remove(string code);
    List<CappedDeductionAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedDeductionAssignmentStore(Dictionary<string, CappedDeductionAssignment> items, List<string> order) : ICappedDeductionAssignmentStore
{
    public Dictionary<string, CappedDeductionAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedDeductionAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedDeductionAssignment item)
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

    public List<CappedDeductionAssignment> List()
    {
        var outValue = new List<CappedDeductionAssignment>();
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
public sealed class CachedCappedDeductionAssignmentStore(MemoryCappedDeductionAssignmentStore inner, CappedDeductionAssignmentCache cache, int hits, int misses) : ICappedDeductionAssignmentStore
{
    public MemoryCappedDeductionAssignmentStore Inner { get; set; } = inner;
    public CappedDeductionAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedDeductionAssignment? Find(string code)
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

    public void Save(CappedDeductionAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedDeductionAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped deduction assignments. Empty fields match all.</summary>
public sealed record CappedDeductionAssignmentQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(CappedDeductionAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<CappedDeductionAssignment> Apply(List<CappedDeductionAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedDeductionAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped deduction assignment store.</summary>
public static class CappedDeductionAssignmentStore
{
    public static ICappedDeductionAssignmentStore NewMemoryCappedDeductionAssignmentStore()
    {
        return new MemoryCappedDeductionAssignmentStore(
            new Dictionary<string, CappedDeductionAssignment>(),
            new List<string>());
    }

    public static ICappedDeductionAssignmentStore NewCachedCappedDeductionAssignmentStore(MemoryCappedDeductionAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedDeductionAssignmentStore(
            inner,
            CappedDeductionAssignmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedDeductionAssignment> FindCappedDeductionAssignments(ICappedDeductionAssignmentStore store, CappedDeductionAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
