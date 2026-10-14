namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive garnishment plans by code.</summary>
public interface IAdaptiveGarnishmentPlanStore
{
    AdaptiveGarnishmentPlan? Find(string code);
    void Save(AdaptiveGarnishmentPlan item);
    bool Remove(string code);
    List<AdaptiveGarnishmentPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveGarnishmentPlanStore(Dictionary<string, AdaptiveGarnishmentPlan> items, List<string> order) : IAdaptiveGarnishmentPlanStore
{
    public Dictionary<string, AdaptiveGarnishmentPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveGarnishmentPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveGarnishmentPlan item)
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

    public List<AdaptiveGarnishmentPlan> List()
    {
        var outValue = new List<AdaptiveGarnishmentPlan>();
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
public sealed class CachedAdaptiveGarnishmentPlanStore(MemoryAdaptiveGarnishmentPlanStore inner, AdaptiveGarnishmentPlanCache cache, int hits, int misses) : IAdaptiveGarnishmentPlanStore
{
    public MemoryAdaptiveGarnishmentPlanStore Inner { get; set; } = inner;
    public AdaptiveGarnishmentPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveGarnishmentPlan? Find(string code)
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

    public void Save(AdaptiveGarnishmentPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveGarnishmentPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive garnishment plans. Empty fields match all.</summary>
public sealed record AdaptiveGarnishmentPlanQuery(string Region, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveGarnishmentPlan item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<AdaptiveGarnishmentPlan> Apply(List<AdaptiveGarnishmentPlan> items)
    {
        var matched = Collections.Filter(items, (AdaptiveGarnishmentPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive garnishment plan store.</summary>
public static class AdaptiveGarnishmentPlanStore
{
    public static IAdaptiveGarnishmentPlanStore NewMemoryAdaptiveGarnishmentPlanStore()
    {
        return new MemoryAdaptiveGarnishmentPlanStore(
            new Dictionary<string, AdaptiveGarnishmentPlan>(),
            new List<string>());
    }

    public static IAdaptiveGarnishmentPlanStore NewCachedAdaptiveGarnishmentPlanStore(MemoryAdaptiveGarnishmentPlanStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveGarnishmentPlanStore(
            inner,
            AdaptiveGarnishmentPlanCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveGarnishmentPlan> FindAdaptiveGarnishmentPlans(IAdaptiveGarnishmentPlanStore store, AdaptiveGarnishmentPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
