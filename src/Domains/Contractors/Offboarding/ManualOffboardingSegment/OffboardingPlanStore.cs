namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding plans by code.</summary>
public interface IOffboardingPlanStore
{
    OffboardingPlan? Find(string code);
    void Save(OffboardingPlan item);
    bool Remove(string code);
    List<OffboardingPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingPlanStore(Dictionary<string, OffboardingPlan> items, List<string> order) : IOffboardingPlanStore
{
    public Dictionary<string, OffboardingPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingPlan item)
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

    public List<OffboardingPlan> List()
    {
        var outValue = new List<OffboardingPlan>();
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
public sealed class CachedOffboardingPlanStore(MemoryOffboardingPlanStore inner, OffboardingPlanCache cache, int hits, int misses) : IOffboardingPlanStore
{
    public MemoryOffboardingPlanStore Inner { get; set; } = inner;
    public OffboardingPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingPlan? Find(string code)
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

    public void Save(OffboardingPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding plans. Empty fields match all.</summary>
public sealed record OffboardingPlanQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingPlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingPlan> Apply(List<OffboardingPlan> items)
    {
        var matched = Collections.Filter(items, (OffboardingPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding plan store.</summary>
public static class OffboardingPlanStore
{
    public static IOffboardingPlanStore NewMemoryOffboardingPlanStore()
    {
        return new MemoryOffboardingPlanStore(new Dictionary<string, OffboardingPlan>(), new List<string>());
    }

    public static IOffboardingPlanStore NewCachedOffboardingPlanStore(MemoryOffboardingPlanStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingPlanStore(inner, OffboardingPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<OffboardingPlan> FindOffboardingPlans(IOffboardingPlanStore store, OffboardingPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
