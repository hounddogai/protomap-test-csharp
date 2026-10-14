namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual scenario assignments by code.</summary>
public interface IManualScenarioAssignmentStore
{
    ManualScenarioAssignment? Find(string code);
    void Save(ManualScenarioAssignment item);
    bool Remove(string code);
    List<ManualScenarioAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualScenarioAssignmentStore(Dictionary<string, ManualScenarioAssignment> items, List<string> order) : IManualScenarioAssignmentStore
{
    public Dictionary<string, ManualScenarioAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualScenarioAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualScenarioAssignment item)
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

    public List<ManualScenarioAssignment> List()
    {
        var outValue = new List<ManualScenarioAssignment>();
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
public sealed class CachedManualScenarioAssignmentStore(MemoryManualScenarioAssignmentStore inner, ManualScenarioAssignmentCache cache, int hits, int misses) : IManualScenarioAssignmentStore
{
    public MemoryManualScenarioAssignmentStore Inner { get; set; } = inner;
    public ManualScenarioAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualScenarioAssignment? Find(string code)
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

    public void Save(ManualScenarioAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualScenarioAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual scenario assignments. Empty fields match all.</summary>
public sealed record ManualScenarioAssignmentQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ManualScenarioAssignment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualScenarioAssignment> Apply(List<ManualScenarioAssignment> items)
    {
        var matched = Collections.Filter(items, (ManualScenarioAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual scenario assignment store.</summary>
public static class ManualScenarioAssignmentStore
{
    public static IManualScenarioAssignmentStore NewMemoryManualScenarioAssignmentStore()
    {
        return new MemoryManualScenarioAssignmentStore(
            new Dictionary<string, ManualScenarioAssignment>(),
            new List<string>());
    }

    public static IManualScenarioAssignmentStore NewCachedManualScenarioAssignmentStore(MemoryManualScenarioAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedManualScenarioAssignmentStore(
            inner,
            ManualScenarioAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualScenarioAssignment> FindManualScenarioAssignments(IManualScenarioAssignmentStore store, ManualScenarioAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
