namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount checkpoints by code.</summary>
public interface IHeadcountCheckpointStore
{
    HeadcountCheckpoint? Find(string code);
    void Save(HeadcountCheckpoint item);
    bool Remove(string code);
    List<HeadcountCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountCheckpointStore(Dictionary<string, HeadcountCheckpoint> items, List<string> order) : IHeadcountCheckpointStore
{
    public Dictionary<string, HeadcountCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountCheckpoint item)
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

    public List<HeadcountCheckpoint> List()
    {
        var outValue = new List<HeadcountCheckpoint>();
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
public sealed class CachedHeadcountCheckpointStore(MemoryHeadcountCheckpointStore inner, HeadcountCheckpointCache cache, int hits, int misses) : IHeadcountCheckpointStore
{
    public MemoryHeadcountCheckpointStore Inner { get; set; } = inner;
    public HeadcountCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountCheckpoint? Find(string code)
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

    public void Save(HeadcountCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount checkpoints. Empty fields match all.</summary>
public sealed record HeadcountCheckpointQuery(string Category, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
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

    public List<HeadcountCheckpoint> Apply(List<HeadcountCheckpoint> items)
    {
        var matched = Collections.Filter(items, (HeadcountCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount checkpoint store.</summary>
public static class HeadcountCheckpointStore
{
    public static IHeadcountCheckpointStore NewMemoryHeadcountCheckpointStore()
    {
        return new MemoryHeadcountCheckpointStore(new Dictionary<string, HeadcountCheckpoint>(), new List<string>());
    }

    public static IHeadcountCheckpointStore NewCachedHeadcountCheckpointStore(MemoryHeadcountCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountCheckpointStore(inner, HeadcountCheckpointCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountCheckpoint> FindHeadcountCheckpoints(IHeadcountCheckpointStore store, HeadcountCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
