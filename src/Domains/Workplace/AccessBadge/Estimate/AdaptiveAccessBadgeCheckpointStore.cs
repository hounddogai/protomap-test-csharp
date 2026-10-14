namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive access badge checkpoints by code.</summary>
public interface IAdaptiveAccessBadgeCheckpointStore
{
    AdaptiveAccessBadgeCheckpoint? Find(string code);
    void Save(AdaptiveAccessBadgeCheckpoint item);
    bool Remove(string code);
    List<AdaptiveAccessBadgeCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveAccessBadgeCheckpointStore(Dictionary<string, AdaptiveAccessBadgeCheckpoint> items, List<string> order) : IAdaptiveAccessBadgeCheckpointStore
{
    public Dictionary<string, AdaptiveAccessBadgeCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveAccessBadgeCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveAccessBadgeCheckpoint item)
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

    public List<AdaptiveAccessBadgeCheckpoint> List()
    {
        var outValue = new List<AdaptiveAccessBadgeCheckpoint>();
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
public sealed class CachedAdaptiveAccessBadgeCheckpointStore(MemoryAdaptiveAccessBadgeCheckpointStore inner, AdaptiveAccessBadgeCheckpointCache cache, int hits, int misses) : IAdaptiveAccessBadgeCheckpointStore
{
    public MemoryAdaptiveAccessBadgeCheckpointStore Inner { get; set; } = inner;
    public AdaptiveAccessBadgeCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveAccessBadgeCheckpoint? Find(string code)
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

    public void Save(AdaptiveAccessBadgeCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveAccessBadgeCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive access badge checkpoints. Empty fields match all.</summary>
public sealed record AdaptiveAccessBadgeCheckpointQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveAccessBadgeCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveAccessBadgeCheckpoint> Apply(List<AdaptiveAccessBadgeCheckpoint> items)
    {
        var matched = Collections.Filter(items, (AdaptiveAccessBadgeCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive access badge checkpoint store.</summary>
public static class AdaptiveAccessBadgeCheckpointStore
{
    public static IAdaptiveAccessBadgeCheckpointStore NewMemoryAdaptiveAccessBadgeCheckpointStore()
    {
        return new MemoryAdaptiveAccessBadgeCheckpointStore(
            new Dictionary<string, AdaptiveAccessBadgeCheckpoint>(),
            new List<string>());
    }

    public static IAdaptiveAccessBadgeCheckpointStore NewCachedAdaptiveAccessBadgeCheckpointStore(MemoryAdaptiveAccessBadgeCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveAccessBadgeCheckpointStore(
            inner,
            AdaptiveAccessBadgeCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveAccessBadgeCheckpoint> FindAdaptiveAccessBadgeCheckpoints(IAdaptiveAccessBadgeCheckpointStore store, AdaptiveAccessBadgeCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
