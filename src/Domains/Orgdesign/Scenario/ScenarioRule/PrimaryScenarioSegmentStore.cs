namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary scenario segments by code.</summary>
public interface IPrimaryScenarioSegmentStore
{
    PrimaryScenarioSegment? Find(string code);
    void Save(PrimaryScenarioSegment item);
    bool Remove(string code);
    List<PrimaryScenarioSegment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryScenarioSegmentStore(Dictionary<string, PrimaryScenarioSegment> items, List<string> order) : IPrimaryScenarioSegmentStore
{
    public Dictionary<string, PrimaryScenarioSegment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryScenarioSegment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryScenarioSegment item)
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

    public List<PrimaryScenarioSegment> List()
    {
        var outValue = new List<PrimaryScenarioSegment>();
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
public sealed class CachedPrimaryScenarioSegmentStore(MemoryPrimaryScenarioSegmentStore inner, PrimaryScenarioSegmentCache cache, int hits, int misses) : IPrimaryScenarioSegmentStore
{
    public MemoryPrimaryScenarioSegmentStore Inner { get; set; } = inner;
    public PrimaryScenarioSegmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryScenarioSegment? Find(string code)
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

    public void Save(PrimaryScenarioSegment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryScenarioSegment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary scenario segments. Empty fields match all.</summary>
public sealed record PrimaryScenarioSegmentQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryScenarioSegment item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryScenarioSegment> Apply(List<PrimaryScenarioSegment> items)
    {
        var matched = Collections.Filter(items, (PrimaryScenarioSegment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary scenario segment store.</summary>
public static class PrimaryScenarioSegmentStore
{
    public static IPrimaryScenarioSegmentStore NewMemoryPrimaryScenarioSegmentStore()
    {
        return new MemoryPrimaryScenarioSegmentStore(
            new Dictionary<string, PrimaryScenarioSegment>(),
            new List<string>());
    }

    public static IPrimaryScenarioSegmentStore NewCachedPrimaryScenarioSegmentStore(MemoryPrimaryScenarioSegmentStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryScenarioSegmentStore(
            inner,
            PrimaryScenarioSegmentCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryScenarioSegment> FindPrimaryScenarioSegments(IPrimaryScenarioSegmentStore store, PrimaryScenarioSegmentQuery query)
    {
        return query.Apply(store.List());
    }
}
