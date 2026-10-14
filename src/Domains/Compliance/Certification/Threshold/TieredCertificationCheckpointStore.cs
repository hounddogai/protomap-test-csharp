namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered certification checkpoints by code.</summary>
public interface ITieredCertificationCheckpointStore
{
    TieredCertificationCheckpoint? Find(string code);
    void Save(TieredCertificationCheckpoint item);
    bool Remove(string code);
    List<TieredCertificationCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredCertificationCheckpointStore(Dictionary<string, TieredCertificationCheckpoint> items, List<string> order) : ITieredCertificationCheckpointStore
{
    public Dictionary<string, TieredCertificationCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredCertificationCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredCertificationCheckpoint item)
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

    public List<TieredCertificationCheckpoint> List()
    {
        var outValue = new List<TieredCertificationCheckpoint>();
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
public sealed class CachedTieredCertificationCheckpointStore(MemoryTieredCertificationCheckpointStore inner, TieredCertificationCheckpointCache cache, int hits, int misses) : ITieredCertificationCheckpointStore
{
    public MemoryTieredCertificationCheckpointStore Inner { get; set; } = inner;
    public TieredCertificationCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredCertificationCheckpoint? Find(string code)
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

    public void Save(TieredCertificationCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredCertificationCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered certification checkpoints. Empty fields match all.</summary>
public sealed record TieredCertificationCheckpointQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TieredCertificationCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredCertificationCheckpoint> Apply(List<TieredCertificationCheckpoint> items)
    {
        var matched = Collections.Filter(items, (TieredCertificationCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered certification checkpoint store.</summary>
public static class TieredCertificationCheckpointStore
{
    public static ITieredCertificationCheckpointStore NewMemoryTieredCertificationCheckpointStore()
    {
        return new MemoryTieredCertificationCheckpointStore(
            new Dictionary<string, TieredCertificationCheckpoint>(),
            new List<string>());
    }

    public static ITieredCertificationCheckpointStore NewCachedTieredCertificationCheckpointStore(MemoryTieredCertificationCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedTieredCertificationCheckpointStore(
            inner,
            TieredCertificationCheckpointCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<TieredCertificationCheckpoint> FindTieredCertificationCheckpoints(ITieredCertificationCheckpointStore store, TieredCertificationCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
