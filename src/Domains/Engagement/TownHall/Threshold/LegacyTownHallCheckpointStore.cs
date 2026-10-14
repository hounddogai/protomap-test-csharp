namespace Acme.Workforce.Engagement.TownHall.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy town hall checkpoints by code.</summary>
public interface ILegacyTownHallCheckpointStore
{
    LegacyTownHallCheckpoint? Find(string code);
    void Save(LegacyTownHallCheckpoint item);
    bool Remove(string code);
    List<LegacyTownHallCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTownHallCheckpointStore(Dictionary<string, LegacyTownHallCheckpoint> items, List<string> order) : ILegacyTownHallCheckpointStore
{
    public Dictionary<string, LegacyTownHallCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTownHallCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTownHallCheckpoint item)
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

    public List<LegacyTownHallCheckpoint> List()
    {
        var outValue = new List<LegacyTownHallCheckpoint>();
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
public sealed class CachedLegacyTownHallCheckpointStore(MemoryLegacyTownHallCheckpointStore inner, LegacyTownHallCheckpointCache cache, int hits, int misses) : ILegacyTownHallCheckpointStore
{
    public MemoryLegacyTownHallCheckpointStore Inner { get; set; } = inner;
    public LegacyTownHallCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTownHallCheckpoint? Find(string code)
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

    public void Save(LegacyTownHallCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTownHallCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy town hall checkpoints. Empty fields match all.</summary>
public sealed record LegacyTownHallCheckpointQuery(string Region, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTownHallCheckpoint item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<LegacyTownHallCheckpoint> Apply(List<LegacyTownHallCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LegacyTownHallCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy town hall checkpoint store.</summary>
public static class LegacyTownHallCheckpointStore
{
    public static ILegacyTownHallCheckpointStore NewMemoryLegacyTownHallCheckpointStore()
    {
        return new MemoryLegacyTownHallCheckpointStore(
            new Dictionary<string, LegacyTownHallCheckpoint>(),
            new List<string>());
    }

    public static ILegacyTownHallCheckpointStore NewCachedLegacyTownHallCheckpointStore(MemoryLegacyTownHallCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTownHallCheckpointStore(
            inner,
            LegacyTownHallCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyTownHallCheckpoint> FindLegacyTownHallCheckpoints(ILegacyTownHallCheckpointStore store, LegacyTownHallCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
