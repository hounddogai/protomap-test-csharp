namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps span checkpoints by code.</summary>
public interface ISpanCheckpointStore
{
    SpanCheckpoint? Find(string code);
    void Save(SpanCheckpoint item);
    bool Remove(string code);
    List<SpanCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySpanCheckpointStore(Dictionary<string, SpanCheckpoint> items, List<string> order) : ISpanCheckpointStore
{
    public Dictionary<string, SpanCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SpanCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SpanCheckpoint item)
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

    public List<SpanCheckpoint> List()
    {
        var outValue = new List<SpanCheckpoint>();
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
public sealed class CachedSpanCheckpointStore(MemorySpanCheckpointStore inner, SpanCheckpointCache cache, int hits, int misses) : ISpanCheckpointStore
{
    public MemorySpanCheckpointStore Inner { get; set; } = inner;
    public SpanCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SpanCheckpoint? Find(string code)
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

    public void Save(SpanCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SpanCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects span checkpoints. Empty fields match all.</summary>
public sealed record SpanCheckpointQuery(string Name, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(SpanCheckpoint item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<SpanCheckpoint> Apply(List<SpanCheckpoint> items)
    {
        var matched = Collections.Filter(items, (SpanCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of span checkpoint store.</summary>
public static class SpanCheckpointStore
{
    public static ISpanCheckpointStore NewMemorySpanCheckpointStore()
    {
        return new MemorySpanCheckpointStore(new Dictionary<string, SpanCheckpoint>(), new List<string>());
    }

    public static ISpanCheckpointStore NewCachedSpanCheckpointStore(MemorySpanCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedSpanCheckpointStore(inner, SpanCheckpointCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SpanCheckpoint> FindSpanCheckpoints(ISpanCheckpointStore store, SpanCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
