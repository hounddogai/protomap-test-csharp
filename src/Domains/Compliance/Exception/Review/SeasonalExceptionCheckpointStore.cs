namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal exception checkpoints by code.</summary>
public interface ISeasonalExceptionCheckpointStore
{
    SeasonalExceptionCheckpoint? Find(string code);
    void Save(SeasonalExceptionCheckpoint item);
    bool Remove(string code);
    List<SeasonalExceptionCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalExceptionCheckpointStore(Dictionary<string, SeasonalExceptionCheckpoint> items, List<string> order) : ISeasonalExceptionCheckpointStore
{
    public Dictionary<string, SeasonalExceptionCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalExceptionCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalExceptionCheckpoint item)
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

    public List<SeasonalExceptionCheckpoint> List()
    {
        var outValue = new List<SeasonalExceptionCheckpoint>();
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
public sealed class CachedSeasonalExceptionCheckpointStore(MemorySeasonalExceptionCheckpointStore inner, SeasonalExceptionCheckpointCache cache, int hits, int misses) : ISeasonalExceptionCheckpointStore
{
    public MemorySeasonalExceptionCheckpointStore Inner { get; set; } = inner;
    public SeasonalExceptionCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalExceptionCheckpoint? Find(string code)
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

    public void Save(SeasonalExceptionCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalExceptionCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal exception checkpoints. Empty fields match all.</summary>
public sealed record SeasonalExceptionCheckpointQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalExceptionCheckpoint item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalExceptionCheckpoint> Apply(List<SeasonalExceptionCheckpoint> items)
    {
        var matched = Collections.Filter(items, (SeasonalExceptionCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal exception checkpoint store.</summary>
public static class SeasonalExceptionCheckpointStore
{
    public static ISeasonalExceptionCheckpointStore NewMemorySeasonalExceptionCheckpointStore()
    {
        return new MemorySeasonalExceptionCheckpointStore(
            new Dictionary<string, SeasonalExceptionCheckpoint>(),
            new List<string>());
    }

    public static ISeasonalExceptionCheckpointStore NewCachedSeasonalExceptionCheckpointStore(MemorySeasonalExceptionCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalExceptionCheckpointStore(
            inner,
            SeasonalExceptionCheckpointCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalExceptionCheckpoint> FindSeasonalExceptionCheckpoints(ISeasonalExceptionCheckpointStore store, SeasonalExceptionCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
