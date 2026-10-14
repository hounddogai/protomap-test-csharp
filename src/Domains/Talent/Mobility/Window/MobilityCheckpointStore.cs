namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility checkpoints by code.</summary>
public interface IMobilityCheckpointStore
{
    MobilityCheckpoint? Find(string code);
    void Save(MobilityCheckpoint item);
    bool Remove(string code);
    List<MobilityCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityCheckpointStore(Dictionary<string, MobilityCheckpoint> items, List<string> order) : IMobilityCheckpointStore
{
    public Dictionary<string, MobilityCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityCheckpoint item)
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

    public List<MobilityCheckpoint> List()
    {
        var outValue = new List<MobilityCheckpoint>();
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
public sealed class CachedMobilityCheckpointStore(MemoryMobilityCheckpointStore inner, MobilityCheckpointCache cache, int hits, int misses) : IMobilityCheckpointStore
{
    public MemoryMobilityCheckpointStore Inner { get; set; } = inner;
    public MobilityCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityCheckpoint? Find(string code)
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

    public void Save(MobilityCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility checkpoints. Empty fields match all.</summary>
public sealed record MobilityCheckpointQuery(string Category, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilityCheckpoint> Apply(List<MobilityCheckpoint> items)
    {
        var matched = Collections.Filter(items, (MobilityCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility checkpoint store.</summary>
public static class MobilityCheckpointStore
{
    public static IMobilityCheckpointStore NewMemoryMobilityCheckpointStore()
    {
        return new MemoryMobilityCheckpointStore(new Dictionary<string, MobilityCheckpoint>(), new List<string>());
    }

    public static IMobilityCheckpointStore NewCachedMobilityCheckpointStore(MemoryMobilityCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityCheckpointStore(inner, MobilityCheckpointCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MobilityCheckpoint> FindMobilityCheckpoints(IMobilityCheckpointStore store, MobilityCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
