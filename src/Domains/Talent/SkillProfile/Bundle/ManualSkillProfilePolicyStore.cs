namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual skill profile policies by code.</summary>
public interface IManualSkillProfilePolicyStore
{
    ManualSkillProfilePolicy? Find(string code);
    void Save(ManualSkillProfilePolicy item);
    bool Remove(string code);
    List<ManualSkillProfilePolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualSkillProfilePolicyStore(Dictionary<string, ManualSkillProfilePolicy> items, List<string> order) : IManualSkillProfilePolicyStore
{
    public Dictionary<string, ManualSkillProfilePolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualSkillProfilePolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualSkillProfilePolicy item)
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

    public List<ManualSkillProfilePolicy> List()
    {
        var outValue = new List<ManualSkillProfilePolicy>();
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
public sealed class CachedManualSkillProfilePolicyStore(MemoryManualSkillProfilePolicyStore inner, ManualSkillProfilePolicyCache cache, int hits, int misses) : IManualSkillProfilePolicyStore
{
    public MemoryManualSkillProfilePolicyStore Inner { get; set; } = inner;
    public ManualSkillProfilePolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualSkillProfilePolicy? Find(string code)
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

    public void Save(ManualSkillProfilePolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualSkillProfilePolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual skill profile policies. Empty fields match all.</summary>
public sealed record ManualSkillProfilePolicyQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ManualSkillProfilePolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualSkillProfilePolicy> Apply(List<ManualSkillProfilePolicy> items)
    {
        var matched = Collections.Filter(items, (ManualSkillProfilePolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual skill profile policy store.</summary>
public static class ManualSkillProfilePolicyStore
{
    public static IManualSkillProfilePolicyStore NewMemoryManualSkillProfilePolicyStore()
    {
        return new MemoryManualSkillProfilePolicyStore(
            new Dictionary<string, ManualSkillProfilePolicy>(),
            new List<string>());
    }

    public static IManualSkillProfilePolicyStore NewCachedManualSkillProfilePolicyStore(MemoryManualSkillProfilePolicyStore inner, int ttl, IClock clock)
    {
        return new CachedManualSkillProfilePolicyStore(
            inner,
            ManualSkillProfilePolicyCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualSkillProfilePolicy> FindManualSkillProfilePolicies(IManualSkillProfilePolicyStore store, ManualSkillProfilePolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
