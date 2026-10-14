namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped license seat checkpoints by code.</summary>
public interface ICappedLicenseSeatCheckpointStore
{
    CappedLicenseSeatCheckpoint? Find(string code);
    void Save(CappedLicenseSeatCheckpoint item);
    bool Remove(string code);
    List<CappedLicenseSeatCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLicenseSeatCheckpointStore(Dictionary<string, CappedLicenseSeatCheckpoint> items, List<string> order) : ICappedLicenseSeatCheckpointStore
{
    public Dictionary<string, CappedLicenseSeatCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLicenseSeatCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLicenseSeatCheckpoint item)
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

    public List<CappedLicenseSeatCheckpoint> List()
    {
        var outValue = new List<CappedLicenseSeatCheckpoint>();
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
public sealed class CachedCappedLicenseSeatCheckpointStore(MemoryCappedLicenseSeatCheckpointStore inner, CappedLicenseSeatCheckpointCache cache, int hits, int misses) : ICappedLicenseSeatCheckpointStore
{
    public MemoryCappedLicenseSeatCheckpointStore Inner { get; set; } = inner;
    public CappedLicenseSeatCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLicenseSeatCheckpoint? Find(string code)
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

    public void Save(CappedLicenseSeatCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLicenseSeatCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped license seat checkpoints. Empty fields match all.</summary>
public sealed record CappedLicenseSeatCheckpointQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLicenseSeatCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedLicenseSeatCheckpoint> Apply(List<CappedLicenseSeatCheckpoint> items)
    {
        var matched = Collections.Filter(items, (CappedLicenseSeatCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped license seat checkpoint store.</summary>
public static class CappedLicenseSeatCheckpointStore
{
    public static ICappedLicenseSeatCheckpointStore NewMemoryCappedLicenseSeatCheckpointStore()
    {
        return new MemoryCappedLicenseSeatCheckpointStore(
            new Dictionary<string, CappedLicenseSeatCheckpoint>(),
            new List<string>());
    }

    public static ICappedLicenseSeatCheckpointStore NewCachedCappedLicenseSeatCheckpointStore(MemoryCappedLicenseSeatCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLicenseSeatCheckpointStore(
            inner,
            CappedLicenseSeatCheckpointCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<CappedLicenseSeatCheckpoint> FindCappedLicenseSeatCheckpoints(ICappedLicenseSeatCheckpointStore store, CappedLicenseSeatCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
