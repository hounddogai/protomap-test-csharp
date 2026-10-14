namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback certification checkpoints by code.</summary>
public interface IFallbackCertificationCheckpointStore
{
    FallbackCertificationCheckpoint? Find(string code);
    void Save(FallbackCertificationCheckpoint item);
    bool Remove(string code);
    List<FallbackCertificationCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackCertificationCheckpointStore(Dictionary<string, FallbackCertificationCheckpoint> items, List<string> order) : IFallbackCertificationCheckpointStore
{
    public Dictionary<string, FallbackCertificationCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackCertificationCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackCertificationCheckpoint item)
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

    public List<FallbackCertificationCheckpoint> List()
    {
        var outValue = new List<FallbackCertificationCheckpoint>();
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
public sealed class CachedFallbackCertificationCheckpointStore(MemoryFallbackCertificationCheckpointStore inner, FallbackCertificationCheckpointCache cache, int hits, int misses) : IFallbackCertificationCheckpointStore
{
    public MemoryFallbackCertificationCheckpointStore Inner { get; set; } = inner;
    public FallbackCertificationCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackCertificationCheckpoint? Find(string code)
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

    public void Save(FallbackCertificationCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackCertificationCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback certification checkpoints. Empty fields match all.</summary>
public sealed record FallbackCertificationCheckpointQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackCertificationCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackCertificationCheckpoint> Apply(List<FallbackCertificationCheckpoint> items)
    {
        var matched = Collections.Filter(items, (FallbackCertificationCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback certification checkpoint store.</summary>
public static class FallbackCertificationCheckpointStore
{
    public static IFallbackCertificationCheckpointStore NewMemoryFallbackCertificationCheckpointStore()
    {
        return new MemoryFallbackCertificationCheckpointStore(
            new Dictionary<string, FallbackCertificationCheckpoint>(),
            new List<string>());
    }

    public static IFallbackCertificationCheckpointStore NewCachedFallbackCertificationCheckpointStore(MemoryFallbackCertificationCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackCertificationCheckpointStore(
            inner,
            FallbackCertificationCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<FallbackCertificationCheckpoint> FindFallbackCertificationCheckpoints(IFallbackCertificationCheckpointStore store, FallbackCertificationCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
