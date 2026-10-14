namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive bonus pool checkpoints by code.</summary>
public interface IAdaptiveBonusPoolCheckpointStore
{
    AdaptiveBonusPoolCheckpoint? Find(string code);
    void Save(AdaptiveBonusPoolCheckpoint item);
    bool Remove(string code);
    List<AdaptiveBonusPoolCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveBonusPoolCheckpointStore(Dictionary<string, AdaptiveBonusPoolCheckpoint> items, List<string> order) : IAdaptiveBonusPoolCheckpointStore
{
    public Dictionary<string, AdaptiveBonusPoolCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveBonusPoolCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveBonusPoolCheckpoint item)
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

    public List<AdaptiveBonusPoolCheckpoint> List()
    {
        var outValue = new List<AdaptiveBonusPoolCheckpoint>();
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
public sealed class CachedAdaptiveBonusPoolCheckpointStore(MemoryAdaptiveBonusPoolCheckpointStore inner, AdaptiveBonusPoolCheckpointCache cache, int hits, int misses) : IAdaptiveBonusPoolCheckpointStore
{
    public MemoryAdaptiveBonusPoolCheckpointStore Inner { get; set; } = inner;
    public AdaptiveBonusPoolCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveBonusPoolCheckpoint? Find(string code)
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

    public void Save(AdaptiveBonusPoolCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveBonusPoolCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive bonus pool checkpoints. Empty fields match all.</summary>
public sealed record AdaptiveBonusPoolCheckpointQuery(string Source, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveBonusPoolCheckpoint item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveBonusPoolCheckpoint> Apply(List<AdaptiveBonusPoolCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AdaptiveBonusPoolCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive bonus pool checkpoint store.</summary>
public static class AdaptiveBonusPoolCheckpointStore
{
    public static IAdaptiveBonusPoolCheckpointStore NewMemoryAdaptiveBonusPoolCheckpointStore()
    {
        return new MemoryAdaptiveBonusPoolCheckpointStore(
            new Dictionary<string, AdaptiveBonusPoolCheckpoint>(),
            new List<string>());
    }

    public static IAdaptiveBonusPoolCheckpointStore NewCachedAdaptiveBonusPoolCheckpointStore(MemoryAdaptiveBonusPoolCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveBonusPoolCheckpointStore(
            inner,
            AdaptiveBonusPoolCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveBonusPoolCheckpoint> FindAdaptiveBonusPoolCheckpoints(IAdaptiveBonusPoolCheckpointStore store, AdaptiveBonusPoolCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
