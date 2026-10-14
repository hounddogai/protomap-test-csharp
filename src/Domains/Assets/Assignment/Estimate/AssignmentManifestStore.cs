namespace Acme.Workforce.Assets.Assignment.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment manifests by code.</summary>
public interface IAssignmentManifestStore
{
    AssignmentManifest? Find(string code);
    void Save(AssignmentManifest item);
    bool Remove(string code);
    List<AssignmentManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentManifestStore(Dictionary<string, AssignmentManifest> items, List<string> order) : IAssignmentManifestStore
{
    public Dictionary<string, AssignmentManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentManifest item)
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

    public List<AssignmentManifest> List()
    {
        var outValue = new List<AssignmentManifest>();
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
public sealed class CachedAssignmentManifestStore(MemoryAssignmentManifestStore inner, AssignmentManifestCache cache, int hits, int misses) : IAssignmentManifestStore
{
    public MemoryAssignmentManifestStore Inner { get; set; } = inner;
    public AssignmentManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentManifest? Find(string code)
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

    public void Save(AssignmentManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment manifests. Empty fields match all.</summary>
public sealed record AssignmentManifestQuery(string Department, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentManifest item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentManifest> Apply(List<AssignmentManifest> items)
    {
        var matched = Collections.Filter(items, (AssignmentManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment manifest store.</summary>
public static class AssignmentManifestStore
{
    public static IAssignmentManifestStore NewMemoryAssignmentManifestStore()
    {
        return new MemoryAssignmentManifestStore(new Dictionary<string, AssignmentManifest>(), new List<string>());
    }

    public static IAssignmentManifestStore NewCachedAssignmentManifestStore(MemoryAssignmentManifestStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentManifestStore(inner, AssignmentManifestCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AssignmentManifest> FindAssignmentManifests(IAssignmentManifestStore store, AssignmentManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
