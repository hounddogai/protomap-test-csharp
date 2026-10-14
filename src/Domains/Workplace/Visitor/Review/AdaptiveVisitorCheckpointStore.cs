namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive visitor checkpoints by code.</summary>
public interface IAdaptiveVisitorCheckpointStore
{
    AdaptiveVisitorCheckpoint? Find(string code);
    void Save(AdaptiveVisitorCheckpoint item);
    bool Remove(string code);
    List<AdaptiveVisitorCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveVisitorCheckpointStore(Dictionary<string, AdaptiveVisitorCheckpoint> items, List<string> order) : IAdaptiveVisitorCheckpointStore
{
    public Dictionary<string, AdaptiveVisitorCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveVisitorCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveVisitorCheckpoint item)
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

    public List<AdaptiveVisitorCheckpoint> List()
    {
        var outValue = new List<AdaptiveVisitorCheckpoint>();
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
public sealed class CachedAdaptiveVisitorCheckpointStore(MemoryAdaptiveVisitorCheckpointStore inner, AdaptiveVisitorCheckpointCache cache, int hits, int misses) : IAdaptiveVisitorCheckpointStore
{
    public MemoryAdaptiveVisitorCheckpointStore Inner { get; set; } = inner;
    public AdaptiveVisitorCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveVisitorCheckpoint? Find(string code)
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

    public void Save(AdaptiveVisitorCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveVisitorCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive visitor checkpoints. Empty fields match all.</summary>
public sealed record AdaptiveVisitorCheckpointQuery(string Country, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveVisitorCheckpoint item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<AdaptiveVisitorCheckpoint> Apply(List<AdaptiveVisitorCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AdaptiveVisitorCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive visitor checkpoint store.</summary>
public static class AdaptiveVisitorCheckpointStore
{
    public static IAdaptiveVisitorCheckpointStore NewMemoryAdaptiveVisitorCheckpointStore()
    {
        return new MemoryAdaptiveVisitorCheckpointStore(
            new Dictionary<string, AdaptiveVisitorCheckpoint>(),
            new List<string>());
    }

    public static IAdaptiveVisitorCheckpointStore NewCachedAdaptiveVisitorCheckpointStore(MemoryAdaptiveVisitorCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveVisitorCheckpointStore(
            inner,
            AdaptiveVisitorCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveVisitorCheckpoint> FindAdaptiveVisitorCheckpoints(IAdaptiveVisitorCheckpointStore store, AdaptiveVisitorCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
