namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile plans by code.</summary>
public interface ISkillProfilePlanStore
{
    SkillProfilePlan? Find(string code);
    void Save(SkillProfilePlan item);
    bool Remove(string code);
    List<SkillProfilePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfilePlanStore(Dictionary<string, SkillProfilePlan> items, List<string> order) : ISkillProfilePlanStore
{
    public Dictionary<string, SkillProfilePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfilePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfilePlan item)
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

    public List<SkillProfilePlan> List()
    {
        var outValue = new List<SkillProfilePlan>();
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
public sealed class CachedSkillProfilePlanStore(MemorySkillProfilePlanStore inner, SkillProfilePlanCache cache, int hits, int misses) : ISkillProfilePlanStore
{
    public MemorySkillProfilePlanStore Inner { get; set; } = inner;
    public SkillProfilePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfilePlan? Find(string code)
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

    public void Save(SkillProfilePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfilePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile plans. Empty fields match all.</summary>
public sealed record SkillProfilePlanQuery(string Label, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfilePlan item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfilePlan> Apply(List<SkillProfilePlan> items)
    {
        var matched = Collections.Filter(items, (SkillProfilePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile plan store.</summary>
public static class SkillProfilePlanStore
{
    public static ISkillProfilePlanStore NewMemorySkillProfilePlanStore()
    {
        return new MemorySkillProfilePlanStore(new Dictionary<string, SkillProfilePlan>(), new List<string>());
    }

    public static ISkillProfilePlanStore NewCachedSkillProfilePlanStore(MemorySkillProfilePlanStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfilePlanStore(inner, SkillProfilePlanCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SkillProfilePlan> FindSkillProfilePlans(ISkillProfilePlanStore store, SkillProfilePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
