namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled accrual assignments by code.</summary>
public interface IScheduledAccrualAssignmentStore
{
    ScheduledAccrualAssignment? Find(string code);
    void Save(ScheduledAccrualAssignment item);
    bool Remove(string code);
    List<ScheduledAccrualAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAccrualAssignmentStore(Dictionary<string, ScheduledAccrualAssignment> items, List<string> order) : IScheduledAccrualAssignmentStore
{
    public Dictionary<string, ScheduledAccrualAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAccrualAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAccrualAssignment item)
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

    public List<ScheduledAccrualAssignment> List()
    {
        var outValue = new List<ScheduledAccrualAssignment>();
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
public sealed class CachedScheduledAccrualAssignmentStore(MemoryScheduledAccrualAssignmentStore inner, ScheduledAccrualAssignmentCache cache, int hits, int misses) : IScheduledAccrualAssignmentStore
{
    public MemoryScheduledAccrualAssignmentStore Inner { get; set; } = inner;
    public ScheduledAccrualAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAccrualAssignment? Find(string code)
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

    public void Save(ScheduledAccrualAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAccrualAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled accrual assignments. Empty fields match all.</summary>
public sealed record ScheduledAccrualAssignmentQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAccrualAssignment item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAccrualAssignment> Apply(List<ScheduledAccrualAssignment> items)
    {
        var matched = Collections.Filter(items, (ScheduledAccrualAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled accrual assignment store.</summary>
public static class ScheduledAccrualAssignmentStore
{
    public static IScheduledAccrualAssignmentStore NewMemoryScheduledAccrualAssignmentStore()
    {
        return new MemoryScheduledAccrualAssignmentStore(
            new Dictionary<string, ScheduledAccrualAssignment>(),
            new List<string>());
    }

    public static IScheduledAccrualAssignmentStore NewCachedScheduledAccrualAssignmentStore(MemoryScheduledAccrualAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAccrualAssignmentStore(
            inner,
            ScheduledAccrualAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledAccrualAssignment> FindScheduledAccrualAssignments(IScheduledAccrualAssignmentStore store, ScheduledAccrualAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
