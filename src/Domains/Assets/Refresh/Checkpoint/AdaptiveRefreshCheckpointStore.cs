namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive refresh checkpoints by code.</summary>
public interface IAdaptiveRefreshCheckpointStore
{
    AdaptiveRefreshCheckpoint? Find(string code);
    void Save(AdaptiveRefreshCheckpoint item);
    bool Remove(string code);
    List<AdaptiveRefreshCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveRefreshCheckpointStore(Dictionary<string, AdaptiveRefreshCheckpoint> items, List<string> order) : IAdaptiveRefreshCheckpointStore
{
    public Dictionary<string, AdaptiveRefreshCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveRefreshCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveRefreshCheckpoint item)
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

    public List<AdaptiveRefreshCheckpoint> List()
    {
        var outValue = new List<AdaptiveRefreshCheckpoint>();
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
public sealed class CachedAdaptiveRefreshCheckpointStore(MemoryAdaptiveRefreshCheckpointStore inner, AdaptiveRefreshCheckpointCache cache, int hits, int misses) : IAdaptiveRefreshCheckpointStore
{
    public MemoryAdaptiveRefreshCheckpointStore Inner { get; set; } = inner;
    public AdaptiveRefreshCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveRefreshCheckpoint? Find(string code)
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

    public void Save(AdaptiveRefreshCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveRefreshCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive refresh checkpoints. Empty fields match all.</summary>
public sealed record AdaptiveRefreshCheckpointQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveRefreshCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveRefreshCheckpoint> Apply(List<AdaptiveRefreshCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AdaptiveRefreshCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive refresh checkpoint store.</summary>
public static class AdaptiveRefreshCheckpointStore
{
    public static IAdaptiveRefreshCheckpointStore NewMemoryAdaptiveRefreshCheckpointStore()
    {
        return new MemoryAdaptiveRefreshCheckpointStore(
            new Dictionary<string, AdaptiveRefreshCheckpoint>(),
            new List<string>());
    }

    public static IAdaptiveRefreshCheckpointStore NewCachedAdaptiveRefreshCheckpointStore(MemoryAdaptiveRefreshCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveRefreshCheckpointStore(
            inner,
            AdaptiveRefreshCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveRefreshCheckpoint> FindAdaptiveRefreshCheckpoints(IAdaptiveRefreshCheckpointStore store, AdaptiveRefreshCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
