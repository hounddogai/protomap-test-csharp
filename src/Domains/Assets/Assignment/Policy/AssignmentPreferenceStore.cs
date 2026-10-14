namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment preferences by code.</summary>
public interface IAssignmentPreferenceStore
{
    AssignmentPreference? Find(string code);
    void Save(AssignmentPreference item);
    bool Remove(string code);
    List<AssignmentPreference> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentPreferenceStore(Dictionary<string, AssignmentPreference> items, List<string> order) : IAssignmentPreferenceStore
{
    public Dictionary<string, AssignmentPreference> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentPreference? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentPreference item)
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

    public List<AssignmentPreference> List()
    {
        var outValue = new List<AssignmentPreference>();
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
public sealed class CachedAssignmentPreferenceStore(MemoryAssignmentPreferenceStore inner, AssignmentPreferenceCache cache, int hits, int misses) : IAssignmentPreferenceStore
{
    public MemoryAssignmentPreferenceStore Inner { get; set; } = inner;
    public AssignmentPreferenceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentPreference? Find(string code)
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

    public void Save(AssignmentPreference item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentPreference> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment preferences. Empty fields match all.</summary>
public sealed record AssignmentPreferenceQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentPreference item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentPreference> Apply(List<AssignmentPreference> items)
    {
        var matched = Collections.Filter(items, (AssignmentPreference candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment preference store.</summary>
public static class AssignmentPreferenceStore
{
    public static IAssignmentPreferenceStore NewMemoryAssignmentPreferenceStore()
    {
        return new MemoryAssignmentPreferenceStore(
            new Dictionary<string, AssignmentPreference>(),
            new List<string>());
    }

    public static IAssignmentPreferenceStore NewCachedAssignmentPreferenceStore(MemoryAssignmentPreferenceStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentPreferenceStore(inner, AssignmentPreferenceCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<AssignmentPreference> FindAssignmentPreferences(IAssignmentPreferenceStore store, AssignmentPreferenceQuery query)
    {
        return query.Apply(store.List());
    }
}
