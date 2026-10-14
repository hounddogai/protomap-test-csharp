namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive retro pay checkpoints by code.</summary>
public interface IAdaptiveRetroPayCheckpointStore
{
    AdaptiveRetroPayCheckpoint? Find(string code);
    void Save(AdaptiveRetroPayCheckpoint item);
    bool Remove(string code);
    List<AdaptiveRetroPayCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveRetroPayCheckpointStore(Dictionary<string, AdaptiveRetroPayCheckpoint> items, List<string> order) : IAdaptiveRetroPayCheckpointStore
{
    public Dictionary<string, AdaptiveRetroPayCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveRetroPayCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveRetroPayCheckpoint item)
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

    public List<AdaptiveRetroPayCheckpoint> List()
    {
        var outValue = new List<AdaptiveRetroPayCheckpoint>();
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
public sealed class CachedAdaptiveRetroPayCheckpointStore(MemoryAdaptiveRetroPayCheckpointStore inner, AdaptiveRetroPayCheckpointCache cache, int hits, int misses) : IAdaptiveRetroPayCheckpointStore
{
    public MemoryAdaptiveRetroPayCheckpointStore Inner { get; set; } = inner;
    public AdaptiveRetroPayCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveRetroPayCheckpoint? Find(string code)
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

    public void Save(AdaptiveRetroPayCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveRetroPayCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive retro pay checkpoints. Empty fields match all.</summary>
public sealed record AdaptiveRetroPayCheckpointQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveRetroPayCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveRetroPayCheckpoint> Apply(List<AdaptiveRetroPayCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AdaptiveRetroPayCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive retro pay checkpoint store.</summary>
public static class AdaptiveRetroPayCheckpointStore
{
    public static IAdaptiveRetroPayCheckpointStore NewMemoryAdaptiveRetroPayCheckpointStore()
    {
        return new MemoryAdaptiveRetroPayCheckpointStore(
            new Dictionary<string, AdaptiveRetroPayCheckpoint>(),
            new List<string>());
    }

    public static IAdaptiveRetroPayCheckpointStore NewCachedAdaptiveRetroPayCheckpointStore(MemoryAdaptiveRetroPayCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveRetroPayCheckpointStore(
            inner,
            AdaptiveRetroPayCheckpointCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveRetroPayCheckpoint> FindAdaptiveRetroPayCheckpoints(IAdaptiveRetroPayCheckpointStore store, AdaptiveRetroPayCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
