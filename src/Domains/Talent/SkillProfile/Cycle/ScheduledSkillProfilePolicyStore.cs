namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled skill profile policies by code.</summary>
public interface IScheduledSkillProfilePolicyStore
{
    ScheduledSkillProfilePolicy? Find(string code);
    void Save(ScheduledSkillProfilePolicy item);
    bool Remove(string code);
    List<ScheduledSkillProfilePolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledSkillProfilePolicyStore(Dictionary<string, ScheduledSkillProfilePolicy> items, List<string> order) : IScheduledSkillProfilePolicyStore
{
    public Dictionary<string, ScheduledSkillProfilePolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledSkillProfilePolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledSkillProfilePolicy item)
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

    public List<ScheduledSkillProfilePolicy> List()
    {
        var outValue = new List<ScheduledSkillProfilePolicy>();
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
public sealed class CachedScheduledSkillProfilePolicyStore(MemoryScheduledSkillProfilePolicyStore inner, ScheduledSkillProfilePolicyCache cache, int hits, int misses) : IScheduledSkillProfilePolicyStore
{
    public MemoryScheduledSkillProfilePolicyStore Inner { get; set; } = inner;
    public ScheduledSkillProfilePolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledSkillProfilePolicy? Find(string code)
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

    public void Save(ScheduledSkillProfilePolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledSkillProfilePolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled skill profile policies. Empty fields match all.</summary>
public sealed record ScheduledSkillProfilePolicyQuery(string Segment, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledSkillProfilePolicy item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledSkillProfilePolicy> Apply(List<ScheduledSkillProfilePolicy> items)
    {
        var matched = Collections.Filter(items, (ScheduledSkillProfilePolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled skill profile policy store.</summary>
public static class ScheduledSkillProfilePolicyStore
{
    public static IScheduledSkillProfilePolicyStore NewMemoryScheduledSkillProfilePolicyStore()
    {
        return new MemoryScheduledSkillProfilePolicyStore(
            new Dictionary<string, ScheduledSkillProfilePolicy>(),
            new List<string>());
    }

    public static IScheduledSkillProfilePolicyStore NewCachedScheduledSkillProfilePolicyStore(MemoryScheduledSkillProfilePolicyStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledSkillProfilePolicyStore(
            inner,
            ScheduledSkillProfilePolicyCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledSkillProfilePolicy> FindScheduledSkillProfilePolicies(IScheduledSkillProfilePolicyStore store, ScheduledSkillProfilePolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
