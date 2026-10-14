namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment plans by code.</summary>
public interface IGarnishmentPlanStore
{
    GarnishmentPlan? Find(string code);
    void Save(GarnishmentPlan item);
    bool Remove(string code);
    List<GarnishmentPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentPlanStore(Dictionary<string, GarnishmentPlan> items, List<string> order) : IGarnishmentPlanStore
{
    public Dictionary<string, GarnishmentPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentPlan item)
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

    public List<GarnishmentPlan> List()
    {
        var outValue = new List<GarnishmentPlan>();
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
public sealed class CachedGarnishmentPlanStore(MemoryGarnishmentPlanStore inner, GarnishmentPlanCache cache, int hits, int misses) : IGarnishmentPlanStore
{
    public MemoryGarnishmentPlanStore Inner { get; set; } = inner;
    public GarnishmentPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentPlan? Find(string code)
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

    public void Save(GarnishmentPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment plans. Empty fields match all.</summary>
public sealed record GarnishmentPlanQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentPlan> Apply(List<GarnishmentPlan> items)
    {
        var matched = Collections.Filter(items, (GarnishmentPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment plan store.</summary>
public static class GarnishmentPlanStore
{
    public static IGarnishmentPlanStore NewMemoryGarnishmentPlanStore()
    {
        return new MemoryGarnishmentPlanStore(new Dictionary<string, GarnishmentPlan>(), new List<string>());
    }

    public static IGarnishmentPlanStore NewCachedGarnishmentPlanStore(MemoryGarnishmentPlanStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentPlanStore(inner, GarnishmentPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentPlan> FindGarnishmentPlans(IGarnishmentPlanStore store, GarnishmentPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
