namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment policies by code.</summary>
public interface IAssignmentPolicyStore
{
    AssignmentPolicy? Find(string code);
    void Save(AssignmentPolicy item);
    bool Remove(string code);
    List<AssignmentPolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentPolicyStore(Dictionary<string, AssignmentPolicy> items, List<string> order) : IAssignmentPolicyStore
{
    public Dictionary<string, AssignmentPolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentPolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentPolicy item)
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

    public List<AssignmentPolicy> List()
    {
        var outValue = new List<AssignmentPolicy>();
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
public sealed class CachedAssignmentPolicyStore(MemoryAssignmentPolicyStore inner, AssignmentPolicyCache cache, int hits, int misses) : IAssignmentPolicyStore
{
    public MemoryAssignmentPolicyStore Inner { get; set; } = inner;
    public AssignmentPolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentPolicy? Find(string code)
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

    public void Save(AssignmentPolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentPolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment policies. Empty fields match all.</summary>
public sealed record AssignmentPolicyQuery(string Title, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentPolicy item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentPolicy> Apply(List<AssignmentPolicy> items)
    {
        var matched = Collections.Filter(items, (AssignmentPolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment policy store.</summary>
public static class AssignmentPolicyStore
{
    public static IAssignmentPolicyStore NewMemoryAssignmentPolicyStore()
    {
        return new MemoryAssignmentPolicyStore(new Dictionary<string, AssignmentPolicy>(), new List<string>());
    }

    public static IAssignmentPolicyStore NewCachedAssignmentPolicyStore(MemoryAssignmentPolicyStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentPolicyStore(inner, AssignmentPolicyCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssignmentPolicy> FindAssignmentPolicies(IAssignmentPolicyStore store, AssignmentPolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
