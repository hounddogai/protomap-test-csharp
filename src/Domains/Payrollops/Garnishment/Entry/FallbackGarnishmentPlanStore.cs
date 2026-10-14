namespace Acme.Workforce.Payrollops.Garnishment.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback garnishment plans by code.</summary>
public interface IFallbackGarnishmentPlanStore
{
    FallbackGarnishmentPlan? Find(string code);
    void Save(FallbackGarnishmentPlan item);
    bool Remove(string code);
    List<FallbackGarnishmentPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackGarnishmentPlanStore(Dictionary<string, FallbackGarnishmentPlan> items, List<string> order) : IFallbackGarnishmentPlanStore
{
    public Dictionary<string, FallbackGarnishmentPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackGarnishmentPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackGarnishmentPlan item)
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

    public List<FallbackGarnishmentPlan> List()
    {
        var outValue = new List<FallbackGarnishmentPlan>();
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
public sealed class CachedFallbackGarnishmentPlanStore(MemoryFallbackGarnishmentPlanStore inner, FallbackGarnishmentPlanCache cache, int hits, int misses) : IFallbackGarnishmentPlanStore
{
    public MemoryFallbackGarnishmentPlanStore Inner { get; set; } = inner;
    public FallbackGarnishmentPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackGarnishmentPlan? Find(string code)
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

    public void Save(FallbackGarnishmentPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackGarnishmentPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback garnishment plans. Empty fields match all.</summary>
public sealed record FallbackGarnishmentPlanQuery(string Title, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackGarnishmentPlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackGarnishmentPlan> Apply(List<FallbackGarnishmentPlan> items)
    {
        var matched = Collections.Filter(items, (FallbackGarnishmentPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback garnishment plan store.</summary>
public static class FallbackGarnishmentPlanStore
{
    public static IFallbackGarnishmentPlanStore NewMemoryFallbackGarnishmentPlanStore()
    {
        return new MemoryFallbackGarnishmentPlanStore(
            new Dictionary<string, FallbackGarnishmentPlan>(),
            new List<string>());
    }

    public static IFallbackGarnishmentPlanStore NewCachedFallbackGarnishmentPlanStore(MemoryFallbackGarnishmentPlanStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackGarnishmentPlanStore(
            inner,
            FallbackGarnishmentPlanCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackGarnishmentPlan> FindFallbackGarnishmentPlans(IFallbackGarnishmentPlanStore store, FallbackGarnishmentPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
