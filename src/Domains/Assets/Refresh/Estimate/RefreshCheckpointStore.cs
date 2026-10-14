namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh checkpoints by code.</summary>
public interface IRefreshCheckpointStore
{
    RefreshCheckpoint? Find(string code);
    void Save(RefreshCheckpoint item);
    bool Remove(string code);
    List<RefreshCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshCheckpointStore(Dictionary<string, RefreshCheckpoint> items, List<string> order) : IRefreshCheckpointStore
{
    public Dictionary<string, RefreshCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshCheckpoint item)
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

    public List<RefreshCheckpoint> List()
    {
        var outValue = new List<RefreshCheckpoint>();
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
public sealed class CachedRefreshCheckpointStore(MemoryRefreshCheckpointStore inner, RefreshCheckpointCache cache, int hits, int misses) : IRefreshCheckpointStore
{
    public MemoryRefreshCheckpointStore Inner { get; set; } = inner;
    public RefreshCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshCheckpoint? Find(string code)
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

    public void Save(RefreshCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh checkpoints. Empty fields match all.</summary>
public sealed record RefreshCheckpointQuery(string Locale, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshCheckpoint item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshCheckpoint> Apply(List<RefreshCheckpoint> items)
    {
        var matched = Collections.Filter(items, (RefreshCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh checkpoint store.</summary>
public static class RefreshCheckpointStore
{
    public static IRefreshCheckpointStore NewMemoryRefreshCheckpointStore()
    {
        return new MemoryRefreshCheckpointStore(new Dictionary<string, RefreshCheckpoint>(), new List<string>());
    }

    public static IRefreshCheckpointStore NewCachedRefreshCheckpointStore(MemoryRefreshCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshCheckpointStore(inner, RefreshCheckpointCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RefreshCheckpoint> FindRefreshCheckpoints(IRefreshCheckpointStore store, RefreshCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
