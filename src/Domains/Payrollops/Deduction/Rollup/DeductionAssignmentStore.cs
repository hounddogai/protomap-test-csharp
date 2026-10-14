namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps deduction assignments by code.</summary>
public interface IDeductionAssignmentStore
{
    DeductionAssignment? Find(string code);
    void Save(DeductionAssignment item);
    bool Remove(string code);
    List<DeductionAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeductionAssignmentStore(Dictionary<string, DeductionAssignment> items, List<string> order) : IDeductionAssignmentStore
{
    public Dictionary<string, DeductionAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeductionAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeductionAssignment item)
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

    public List<DeductionAssignment> List()
    {
        var outValue = new List<DeductionAssignment>();
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
public sealed class CachedDeductionAssignmentStore(MemoryDeductionAssignmentStore inner, DeductionAssignmentCache cache, int hits, int misses) : IDeductionAssignmentStore
{
    public MemoryDeductionAssignmentStore Inner { get; set; } = inner;
    public DeductionAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeductionAssignment? Find(string code)
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

    public void Save(DeductionAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeductionAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects deduction assignments. Empty fields match all.</summary>
public sealed record DeductionAssignmentQuery(string Channel, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(DeductionAssignment item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeductionAssignment> Apply(List<DeductionAssignment> items)
    {
        var matched = Collections.Filter(items, (DeductionAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of deduction assignment store.</summary>
public static class DeductionAssignmentStore
{
    public static IDeductionAssignmentStore NewMemoryDeductionAssignmentStore()
    {
        return new MemoryDeductionAssignmentStore(new Dictionary<string, DeductionAssignment>(), new List<string>());
    }

    public static IDeductionAssignmentStore NewCachedDeductionAssignmentStore(MemoryDeductionAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedDeductionAssignmentStore(inner, DeductionAssignmentCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DeductionAssignment> FindDeductionAssignments(IDeductionAssignmentStore store, DeductionAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
