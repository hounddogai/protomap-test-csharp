namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario snapshots by code.</summary>
public interface IScenarioSnapshotStore
{
    ScenarioSnapshot? Find(string code);
    void Save(ScenarioSnapshot item);
    bool Remove(string code);
    List<ScenarioSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioSnapshotStore(Dictionary<string, ScenarioSnapshot> items, List<string> order) : IScenarioSnapshotStore
{
    public Dictionary<string, ScenarioSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioSnapshot item)
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

    public List<ScenarioSnapshot> List()
    {
        var outValue = new List<ScenarioSnapshot>();
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
public sealed class CachedScenarioSnapshotStore(MemoryScenarioSnapshotStore inner, ScenarioSnapshotCache cache, int hits, int misses) : IScenarioSnapshotStore
{
    public MemoryScenarioSnapshotStore Inner { get; set; } = inner;
    public ScenarioSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioSnapshot? Find(string code)
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

    public void Save(ScenarioSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario snapshots. Empty fields match all.</summary>
public sealed record ScenarioSnapshotQuery(string Department, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioSnapshot item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<ScenarioSnapshot> Apply(List<ScenarioSnapshot> items)
    {
        var matched = Collections.Filter(items, (ScenarioSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario snapshot store.</summary>
public static class ScenarioSnapshotStore
{
    public static IScenarioSnapshotStore NewMemoryScenarioSnapshotStore()
    {
        return new MemoryScenarioSnapshotStore(new Dictionary<string, ScenarioSnapshot>(), new List<string>());
    }

    public static IScenarioSnapshotStore NewCachedScenarioSnapshotStore(MemoryScenarioSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioSnapshotStore(inner, ScenarioSnapshotCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScenarioSnapshot> FindScenarioSnapshots(IScenarioSnapshotStore store, ScenarioSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
