namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment overrides by code.</summary>
public interface IAssignmentOverrideStore
{
    AssignmentOverride? Find(string code);
    void Save(AssignmentOverride item);
    bool Remove(string code);
    List<AssignmentOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentOverrideStore(Dictionary<string, AssignmentOverride> items, List<string> order) : IAssignmentOverrideStore
{
    public Dictionary<string, AssignmentOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentOverride item)
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

    public List<AssignmentOverride> List()
    {
        var outValue = new List<AssignmentOverride>();
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
public sealed class CachedAssignmentOverrideStore(MemoryAssignmentOverrideStore inner, AssignmentOverrideCache cache, int hits, int misses) : IAssignmentOverrideStore
{
    public MemoryAssignmentOverrideStore Inner { get; set; } = inner;
    public AssignmentOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentOverride? Find(string code)
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

    public void Save(AssignmentOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment overrides. Empty fields match all.</summary>
public sealed record AssignmentOverrideQuery(string Team, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentOverride item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentOverride> Apply(List<AssignmentOverride> items)
    {
        var matched = Collections.Filter(items, (AssignmentOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment override store.</summary>
public static class AssignmentOverrideStore
{
    public static IAssignmentOverrideStore NewMemoryAssignmentOverrideStore()
    {
        return new MemoryAssignmentOverrideStore(new Dictionary<string, AssignmentOverride>(), new List<string>());
    }

    public static IAssignmentOverrideStore NewCachedAssignmentOverrideStore(MemoryAssignmentOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentOverrideStore(inner, AssignmentOverrideCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentOverride> FindAssignmentOverrides(IAssignmentOverrideStore store, AssignmentOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
