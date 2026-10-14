namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped exception checkpoints by code.</summary>
public interface ICappedExceptionCheckpointStore
{
    CappedExceptionCheckpoint? Find(string code);
    void Save(CappedExceptionCheckpoint item);
    bool Remove(string code);
    List<CappedExceptionCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedExceptionCheckpointStore(Dictionary<string, CappedExceptionCheckpoint> items, List<string> order) : ICappedExceptionCheckpointStore
{
    public Dictionary<string, CappedExceptionCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedExceptionCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedExceptionCheckpoint item)
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

    public List<CappedExceptionCheckpoint> List()
    {
        var outValue = new List<CappedExceptionCheckpoint>();
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
public sealed class CachedCappedExceptionCheckpointStore(MemoryCappedExceptionCheckpointStore inner, CappedExceptionCheckpointCache cache, int hits, int misses) : ICappedExceptionCheckpointStore
{
    public MemoryCappedExceptionCheckpointStore Inner { get; set; } = inner;
    public CappedExceptionCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedExceptionCheckpoint? Find(string code)
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

    public void Save(CappedExceptionCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedExceptionCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped exception checkpoints. Empty fields match all.</summary>
public sealed record CappedExceptionCheckpointQuery(string Title, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(CappedExceptionCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<CappedExceptionCheckpoint> Apply(List<CappedExceptionCheckpoint> items)
    {
        var matched = Collections.Filter(items, (CappedExceptionCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped exception checkpoint store.</summary>
public static class CappedExceptionCheckpointStore
{
    public static ICappedExceptionCheckpointStore NewMemoryCappedExceptionCheckpointStore()
    {
        return new MemoryCappedExceptionCheckpointStore(
            new Dictionary<string, CappedExceptionCheckpoint>(),
            new List<string>());
    }

    public static ICappedExceptionCheckpointStore NewCachedCappedExceptionCheckpointStore(MemoryCappedExceptionCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedCappedExceptionCheckpointStore(
            inner,
            CappedExceptionCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedExceptionCheckpoint> FindCappedExceptionCheckpoints(ICappedExceptionCheckpointStore store, CappedExceptionCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
