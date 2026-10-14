namespace Acme.Workforce.Workplace.Desk.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk checkpoints by code.</summary>
public interface IDeskCheckpointStore
{
    DeskCheckpoint? Find(string code);
    void Save(DeskCheckpoint item);
    bool Remove(string code);
    List<DeskCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskCheckpointStore(Dictionary<string, DeskCheckpoint> items, List<string> order) : IDeskCheckpointStore
{
    public Dictionary<string, DeskCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskCheckpoint item)
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

    public List<DeskCheckpoint> List()
    {
        var outValue = new List<DeskCheckpoint>();
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
public sealed class CachedDeskCheckpointStore(MemoryDeskCheckpointStore inner, DeskCheckpointCache cache, int hits, int misses) : IDeskCheckpointStore
{
    public MemoryDeskCheckpointStore Inner { get; set; } = inner;
    public DeskCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskCheckpoint? Find(string code)
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

    public void Save(DeskCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk checkpoints. Empty fields match all.</summary>
public sealed record DeskCheckpointQuery(string Segment, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(DeskCheckpoint item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskCheckpoint> Apply(List<DeskCheckpoint> items)
    {
        var matched = Collections.Filter(items, (DeskCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk checkpoint store.</summary>
public static class DeskCheckpointStore
{
    public static IDeskCheckpointStore NewMemoryDeskCheckpointStore()
    {
        return new MemoryDeskCheckpointStore(new Dictionary<string, DeskCheckpoint>(), new List<string>());
    }

    public static IDeskCheckpointStore NewCachedDeskCheckpointStore(MemoryDeskCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedDeskCheckpointStore(inner, DeskCheckpointCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DeskCheckpoint> FindDeskCheckpoints(IDeskCheckpointStore store, DeskCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
