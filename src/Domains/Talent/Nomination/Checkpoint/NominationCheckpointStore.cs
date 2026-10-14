namespace Acme.Workforce.Talent.Nomination.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination checkpoints by code.</summary>
public interface INominationCheckpointStore
{
    NominationCheckpoint? Find(string code);
    void Save(NominationCheckpoint item);
    bool Remove(string code);
    List<NominationCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationCheckpointStore(Dictionary<string, NominationCheckpoint> items, List<string> order) : INominationCheckpointStore
{
    public Dictionary<string, NominationCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationCheckpoint item)
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

    public List<NominationCheckpoint> List()
    {
        var outValue = new List<NominationCheckpoint>();
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
public sealed class CachedNominationCheckpointStore(MemoryNominationCheckpointStore inner, NominationCheckpointCache cache, int hits, int misses) : INominationCheckpointStore
{
    public MemoryNominationCheckpointStore Inner { get; set; } = inner;
    public NominationCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationCheckpoint? Find(string code)
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

    public void Save(NominationCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination checkpoints. Empty fields match all.</summary>
public sealed record NominationCheckpointQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(NominationCheckpoint item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationCheckpoint> Apply(List<NominationCheckpoint> items)
    {
        var matched = Collections.Filter(items, (NominationCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination checkpoint store.</summary>
public static class NominationCheckpointStore
{
    public static INominationCheckpointStore NewMemoryNominationCheckpointStore()
    {
        return new MemoryNominationCheckpointStore(
            new Dictionary<string, NominationCheckpoint>(),
            new List<string>());
    }

    public static INominationCheckpointStore NewCachedNominationCheckpointStore(MemoryNominationCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedNominationCheckpointStore(inner, NominationCheckpointCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<NominationCheckpoint> FindNominationCheckpoints(INominationCheckpointStore store, NominationCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
