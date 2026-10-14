namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scenario segments by code.</summary>
public interface IScenarioSegmentStore
{
    ScenarioSegment? Find(string code);
    void Save(ScenarioSegment item);
    bool Remove(string code);
    List<ScenarioSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScenarioSegmentStore(Dictionary<string, ScenarioSegment> items, List<string> order) : IScenarioSegmentStore
{
    public Dictionary<string, ScenarioSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScenarioSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScenarioSegment item)
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

    public List<ScenarioSegment> List()
    {
        var outValue = new List<ScenarioSegment>();
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
public sealed class CachedScenarioSegmentStore(MemoryScenarioSegmentStore inner, ScenarioSegmentCache cache, int hits, int misses) : IScenarioSegmentStore
{
    public MemoryScenarioSegmentStore Inner { get; set; } = inner;
    public ScenarioSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScenarioSegment? Find(string code)
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

    public void Save(ScenarioSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScenarioSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scenario segments. Empty fields match all.</summary>
public sealed record ScenarioSegmentQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(ScenarioSegment item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScenarioSegment> Apply(List<ScenarioSegment> items)
    {
        var matched = Collections.Filter(items, (ScenarioSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scenario segment store.</summary>
public static class ScenarioSegmentStore
{
    public static IScenarioSegmentStore NewMemoryScenarioSegmentStore()
    {
        return new MemoryScenarioSegmentStore(new Dictionary<string, ScenarioSegment>(), new List<string>());
    }

    public static IScenarioSegmentStore NewCachedScenarioSegmentStore(MemoryScenarioSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedScenarioSegmentStore(inner, ScenarioSegmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ScenarioSegment> FindScenarioSegments(IScenarioSegmentStore store, ScenarioSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
