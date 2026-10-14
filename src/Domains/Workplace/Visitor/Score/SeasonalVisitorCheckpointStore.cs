namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal visitor checkpoints by code.</summary>
public interface ISeasonalVisitorCheckpointStore
{
    SeasonalVisitorCheckpoint? Find(string code);
    void Save(SeasonalVisitorCheckpoint item);
    bool Remove(string code);
    List<SeasonalVisitorCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalVisitorCheckpointStore(Dictionary<string, SeasonalVisitorCheckpoint> items, List<string> order) : ISeasonalVisitorCheckpointStore
{
    public Dictionary<string, SeasonalVisitorCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalVisitorCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalVisitorCheckpoint item)
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

    public List<SeasonalVisitorCheckpoint> List()
    {
        var outValue = new List<SeasonalVisitorCheckpoint>();
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
public sealed class CachedSeasonalVisitorCheckpointStore(MemorySeasonalVisitorCheckpointStore inner, SeasonalVisitorCheckpointCache cache, int hits, int misses) : ISeasonalVisitorCheckpointStore
{
    public MemorySeasonalVisitorCheckpointStore Inner { get; set; } = inner;
    public SeasonalVisitorCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalVisitorCheckpoint? Find(string code)
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

    public void Save(SeasonalVisitorCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalVisitorCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal visitor checkpoints. Empty fields match all.</summary>
public sealed record SeasonalVisitorCheckpointQuery(string Locale, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalVisitorCheckpoint item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalVisitorCheckpoint> Apply(List<SeasonalVisitorCheckpoint> items)
    {
        var matched = Collections.Filter(items, (SeasonalVisitorCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal visitor checkpoint store.</summary>
public static class SeasonalVisitorCheckpointStore
{
    public static ISeasonalVisitorCheckpointStore NewMemorySeasonalVisitorCheckpointStore()
    {
        return new MemorySeasonalVisitorCheckpointStore(
            new Dictionary<string, SeasonalVisitorCheckpoint>(),
            new List<string>());
    }

    public static ISeasonalVisitorCheckpointStore NewCachedSeasonalVisitorCheckpointStore(MemorySeasonalVisitorCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalVisitorCheckpointStore(
            inner,
            SeasonalVisitorCheckpointCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalVisitorCheckpoint> FindSeasonalVisitorCheckpoints(ISeasonalVisitorCheckpointStore store, SeasonalVisitorCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
