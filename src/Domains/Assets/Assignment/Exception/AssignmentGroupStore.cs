namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment groups by code.</summary>
public interface IAssignmentGroupStore
{
    AssignmentGroup? Find(string code);
    void Save(AssignmentGroup item);
    bool Remove(string code);
    List<AssignmentGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentGroupStore(Dictionary<string, AssignmentGroup> items, List<string> order) : IAssignmentGroupStore
{
    public Dictionary<string, AssignmentGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentGroup item)
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

    public List<AssignmentGroup> List()
    {
        var outValue = new List<AssignmentGroup>();
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
public sealed class CachedAssignmentGroupStore(MemoryAssignmentGroupStore inner, AssignmentGroupCache cache, int hits, int misses) : IAssignmentGroupStore
{
    public MemoryAssignmentGroupStore Inner { get; set; } = inner;
    public AssignmentGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentGroup? Find(string code)
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

    public void Save(AssignmentGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment groups. Empty fields match all.</summary>
public sealed record AssignmentGroupQuery(string Locale, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentGroup item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentGroup> Apply(List<AssignmentGroup> items)
    {
        var matched = Collections.Filter(items, (AssignmentGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment group store.</summary>
public static class AssignmentGroupStore
{
    public static IAssignmentGroupStore NewMemoryAssignmentGroupStore()
    {
        return new MemoryAssignmentGroupStore(new Dictionary<string, AssignmentGroup>(), new List<string>());
    }

    public static IAssignmentGroupStore NewCachedAssignmentGroupStore(MemoryAssignmentGroupStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentGroupStore(inner, AssignmentGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssignmentGroup> FindAssignmentGroups(IAssignmentGroupStore store, AssignmentGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
