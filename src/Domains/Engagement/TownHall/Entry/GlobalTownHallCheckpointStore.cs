namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global town hall checkpoints by code.</summary>
public interface IGlobalTownHallCheckpointStore
{
    GlobalTownHallCheckpoint? Find(string code);
    void Save(GlobalTownHallCheckpoint item);
    bool Remove(string code);
    List<GlobalTownHallCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalTownHallCheckpointStore(Dictionary<string, GlobalTownHallCheckpoint> items, List<string> order) : IGlobalTownHallCheckpointStore
{
    public Dictionary<string, GlobalTownHallCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalTownHallCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalTownHallCheckpoint item)
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

    public List<GlobalTownHallCheckpoint> List()
    {
        var outValue = new List<GlobalTownHallCheckpoint>();
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
public sealed class CachedGlobalTownHallCheckpointStore(MemoryGlobalTownHallCheckpointStore inner, GlobalTownHallCheckpointCache cache, int hits, int misses) : IGlobalTownHallCheckpointStore
{
    public MemoryGlobalTownHallCheckpointStore Inner { get; set; } = inner;
    public GlobalTownHallCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalTownHallCheckpoint? Find(string code)
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

    public void Save(GlobalTownHallCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalTownHallCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global town hall checkpoints. Empty fields match all.</summary>
public sealed record GlobalTownHallCheckpointQuery(string Team, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalTownHallCheckpoint item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalTownHallCheckpoint> Apply(List<GlobalTownHallCheckpoint> items)
    {
        var matched = Collections.Filter(items, (GlobalTownHallCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global town hall checkpoint store.</summary>
public static class GlobalTownHallCheckpointStore
{
    public static IGlobalTownHallCheckpointStore NewMemoryGlobalTownHallCheckpointStore()
    {
        return new MemoryGlobalTownHallCheckpointStore(
            new Dictionary<string, GlobalTownHallCheckpoint>(),
            new List<string>());
    }

    public static IGlobalTownHallCheckpointStore NewCachedGlobalTownHallCheckpointStore(MemoryGlobalTownHallCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalTownHallCheckpointStore(
            inner,
            GlobalTownHallCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<GlobalTownHallCheckpoint> FindGlobalTownHallCheckpoints(IGlobalTownHallCheckpointStore store, GlobalTownHallCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
