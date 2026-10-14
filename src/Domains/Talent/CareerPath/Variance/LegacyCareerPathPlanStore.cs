namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy career path plans by code.</summary>
public interface ILegacyCareerPathPlanStore
{
    LegacyCareerPathPlan? Find(string code);
    void Save(LegacyCareerPathPlan item);
    bool Remove(string code);
    List<LegacyCareerPathPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyCareerPathPlanStore(Dictionary<string, LegacyCareerPathPlan> items, List<string> order) : ILegacyCareerPathPlanStore
{
    public Dictionary<string, LegacyCareerPathPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyCareerPathPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyCareerPathPlan item)
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

    public List<LegacyCareerPathPlan> List()
    {
        var outValue = new List<LegacyCareerPathPlan>();
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
public sealed class CachedLegacyCareerPathPlanStore(MemoryLegacyCareerPathPlanStore inner, LegacyCareerPathPlanCache cache, int hits, int misses) : ILegacyCareerPathPlanStore
{
    public MemoryLegacyCareerPathPlanStore Inner { get; set; } = inner;
    public LegacyCareerPathPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyCareerPathPlan? Find(string code)
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

    public void Save(LegacyCareerPathPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyCareerPathPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy career path plans. Empty fields match all.</summary>
public sealed record LegacyCareerPathPlanQuery(string Name, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyCareerPathPlan item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyCareerPathPlan> Apply(List<LegacyCareerPathPlan> items)
    {
        var matched = Collections.Filter(items, (LegacyCareerPathPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy career path plan store.</summary>
public static class LegacyCareerPathPlanStore
{
    public static ILegacyCareerPathPlanStore NewMemoryLegacyCareerPathPlanStore()
    {
        return new MemoryLegacyCareerPathPlanStore(
            new Dictionary<string, LegacyCareerPathPlan>(),
            new List<string>());
    }

    public static ILegacyCareerPathPlanStore NewCachedLegacyCareerPathPlanStore(MemoryLegacyCareerPathPlanStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyCareerPathPlanStore(inner, LegacyCareerPathPlanCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyCareerPathPlan> FindLegacyCareerPathPlans(ILegacyCareerPathPlanStore store, LegacyCareerPathPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
