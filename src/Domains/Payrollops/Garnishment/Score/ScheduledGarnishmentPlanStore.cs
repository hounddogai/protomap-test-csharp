namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled garnishment plans by code.</summary>
public interface IScheduledGarnishmentPlanStore
{
    ScheduledGarnishmentPlan? Find(string code);
    void Save(ScheduledGarnishmentPlan item);
    bool Remove(string code);
    List<ScheduledGarnishmentPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledGarnishmentPlanStore(Dictionary<string, ScheduledGarnishmentPlan> items, List<string> order) : IScheduledGarnishmentPlanStore
{
    public Dictionary<string, ScheduledGarnishmentPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledGarnishmentPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledGarnishmentPlan item)
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

    public List<ScheduledGarnishmentPlan> List()
    {
        var outValue = new List<ScheduledGarnishmentPlan>();
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
public sealed class CachedScheduledGarnishmentPlanStore(MemoryScheduledGarnishmentPlanStore inner, ScheduledGarnishmentPlanCache cache, int hits, int misses) : IScheduledGarnishmentPlanStore
{
    public MemoryScheduledGarnishmentPlanStore Inner { get; set; } = inner;
    public ScheduledGarnishmentPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledGarnishmentPlan? Find(string code)
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

    public void Save(ScheduledGarnishmentPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledGarnishmentPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled garnishment plans. Empty fields match all.</summary>
public sealed record ScheduledGarnishmentPlanQuery(string Channel, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledGarnishmentPlan item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledGarnishmentPlan> Apply(List<ScheduledGarnishmentPlan> items)
    {
        var matched = Collections.Filter(items, (ScheduledGarnishmentPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled garnishment plan store.</summary>
public static class ScheduledGarnishmentPlanStore
{
    public static IScheduledGarnishmentPlanStore NewMemoryScheduledGarnishmentPlanStore()
    {
        return new MemoryScheduledGarnishmentPlanStore(
            new Dictionary<string, ScheduledGarnishmentPlan>(),
            new List<string>());
    }

    public static IScheduledGarnishmentPlanStore NewCachedScheduledGarnishmentPlanStore(MemoryScheduledGarnishmentPlanStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledGarnishmentPlanStore(
            inner,
            ScheduledGarnishmentPlanCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledGarnishmentPlan> FindScheduledGarnishmentPlans(IScheduledGarnishmentPlanStore store, ScheduledGarnishmentPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
