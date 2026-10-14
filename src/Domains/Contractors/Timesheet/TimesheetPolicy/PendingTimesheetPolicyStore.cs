namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending timesheet policies by code.</summary>
public interface IPendingTimesheetPolicyStore
{
    PendingTimesheetPolicy? Find(string code);
    void Save(PendingTimesheetPolicy item);
    bool Remove(string code);
    List<PendingTimesheetPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTimesheetPolicyStore(Dictionary<string, PendingTimesheetPolicy> items, List<string> order) : IPendingTimesheetPolicyStore
{
    public Dictionary<string, PendingTimesheetPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTimesheetPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTimesheetPolicy item)
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

    public List<PendingTimesheetPolicy> List()
    {
        var outValue = new List<PendingTimesheetPolicy>();
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
public sealed class CachedPendingTimesheetPolicyStore(MemoryPendingTimesheetPolicyStore inner, PendingTimesheetPolicyCache cache, int hits, int misses) : IPendingTimesheetPolicyStore
{
    public MemoryPendingTimesheetPolicyStore Inner { get; set; } = inner;
    public PendingTimesheetPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTimesheetPolicy? Find(string code)
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

    public void Save(PendingTimesheetPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTimesheetPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending timesheet policies. Empty fields match all.</summary>
public sealed record PendingTimesheetPolicyQuery(string Category, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTimesheetPolicy item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTimesheetPolicy> Apply(List<PendingTimesheetPolicy> items)
    {
        var matched = Collections.Filter(items, (PendingTimesheetPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending timesheet policy store.</summary>
public static class PendingTimesheetPolicyStore
{
    public static IPendingTimesheetPolicyStore NewMemoryPendingTimesheetPolicyStore()
    {
        return new MemoryPendingTimesheetPolicyStore(
            new Dictionary<string, PendingTimesheetPolicy>(),
            new List<string>());
    }

    public static IPendingTimesheetPolicyStore NewCachedPendingTimesheetPolicyStore(MemoryPendingTimesheetPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTimesheetPolicyStore(
            inner,
            PendingTimesheetPolicyCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingTimesheetPolicy> FindPendingTimesheetPolicies(IPendingTimesheetPolicyStore store, PendingTimesheetPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
