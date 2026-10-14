namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment profiles by code.</summary>
public interface IAssignmentProfileStore
{
    AssignmentProfile? Find(string code);
    void Save(AssignmentProfile item);
    bool Remove(string code);
    List<AssignmentProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentProfileStore(Dictionary<string, AssignmentProfile> items, List<string> order) : IAssignmentProfileStore
{
    public Dictionary<string, AssignmentProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentProfile item)
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

    public List<AssignmentProfile> List()
    {
        var outValue = new List<AssignmentProfile>();
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
public sealed class CachedAssignmentProfileStore(MemoryAssignmentProfileStore inner, AssignmentProfileCache cache, int hits, int misses) : IAssignmentProfileStore
{
    public MemoryAssignmentProfileStore Inner { get; set; } = inner;
    public AssignmentProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentProfile? Find(string code)
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

    public void Save(AssignmentProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment profiles. Empty fields match all.</summary>
public sealed record AssignmentProfileQuery(string Category, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentProfile item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentProfile> Apply(List<AssignmentProfile> items)
    {
        var matched = Collections.Filter(items, (AssignmentProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment profile store.</summary>
public static class AssignmentProfileStore
{
    public static IAssignmentProfileStore NewMemoryAssignmentProfileStore()
    {
        return new MemoryAssignmentProfileStore(new Dictionary<string, AssignmentProfile>(), new List<string>());
    }

    public static IAssignmentProfileStore NewCachedAssignmentProfileStore(MemoryAssignmentProfileStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentProfileStore(inner, AssignmentProfileCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AssignmentProfile> FindAssignmentProfiles(IAssignmentProfileStore store, AssignmentProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
