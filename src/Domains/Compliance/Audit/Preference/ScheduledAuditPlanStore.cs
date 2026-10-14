namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled audit plans by code.</summary>
public interface IScheduledAuditPlanStore
{
    ScheduledAuditPlan? Find(string code);
    void Save(ScheduledAuditPlan item);
    bool Remove(string code);
    List<ScheduledAuditPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledAuditPlanStore(Dictionary<string, ScheduledAuditPlan> items, List<string> order) : IScheduledAuditPlanStore
{
    public Dictionary<string, ScheduledAuditPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledAuditPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledAuditPlan item)
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

    public List<ScheduledAuditPlan> List()
    {
        var outValue = new List<ScheduledAuditPlan>();
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
public sealed class CachedScheduledAuditPlanStore(MemoryScheduledAuditPlanStore inner, ScheduledAuditPlanCache cache, int hits, int misses) : IScheduledAuditPlanStore
{
    public MemoryScheduledAuditPlanStore Inner { get; set; } = inner;
    public ScheduledAuditPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledAuditPlan? Find(string code)
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

    public void Save(ScheduledAuditPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledAuditPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled audit plans. Empty fields match all.</summary>
public sealed record ScheduledAuditPlanQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledAuditPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledAuditPlan> Apply(List<ScheduledAuditPlan> items)
    {
        var matched = Collections.Filter(items, (ScheduledAuditPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled audit plan store.</summary>
public static class ScheduledAuditPlanStore
{
    public static IScheduledAuditPlanStore NewMemoryScheduledAuditPlanStore()
    {
        return new MemoryScheduledAuditPlanStore(new Dictionary<string, ScheduledAuditPlan>(), new List<string>());
    }

    public static IScheduledAuditPlanStore NewCachedScheduledAuditPlanStore(MemoryScheduledAuditPlanStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledAuditPlanStore(inner, ScheduledAuditPlanCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ScheduledAuditPlan> FindScheduledAuditPlans(IScheduledAuditPlanStore store, ScheduledAuditPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
