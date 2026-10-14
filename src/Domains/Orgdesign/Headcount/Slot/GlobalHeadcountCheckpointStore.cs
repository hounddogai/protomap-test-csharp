namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global headcount checkpoints by code.</summary>
public interface IGlobalHeadcountCheckpointStore
{
    GlobalHeadcountCheckpoint? Find(string code);
    void Save(GlobalHeadcountCheckpoint item);
    bool Remove(string code);
    List<GlobalHeadcountCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalHeadcountCheckpointStore(Dictionary<string, GlobalHeadcountCheckpoint> items, List<string> order) : IGlobalHeadcountCheckpointStore
{
    public Dictionary<string, GlobalHeadcountCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalHeadcountCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalHeadcountCheckpoint item)
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

    public List<GlobalHeadcountCheckpoint> List()
    {
        var outValue = new List<GlobalHeadcountCheckpoint>();
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
public sealed class CachedGlobalHeadcountCheckpointStore(MemoryGlobalHeadcountCheckpointStore inner, GlobalHeadcountCheckpointCache cache, int hits, int misses) : IGlobalHeadcountCheckpointStore
{
    public MemoryGlobalHeadcountCheckpointStore Inner { get; set; } = inner;
    public GlobalHeadcountCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalHeadcountCheckpoint? Find(string code)
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

    public void Save(GlobalHeadcountCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalHeadcountCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global headcount checkpoints. Empty fields match all.</summary>
public sealed record GlobalHeadcountCheckpointQuery(string Title, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalHeadcountCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalHeadcountCheckpoint> Apply(List<GlobalHeadcountCheckpoint> items)
    {
        var matched = Collections.Filter(items, (GlobalHeadcountCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global headcount checkpoint store.</summary>
public static class GlobalHeadcountCheckpointStore
{
    public static IGlobalHeadcountCheckpointStore NewMemoryGlobalHeadcountCheckpointStore()
    {
        return new MemoryGlobalHeadcountCheckpointStore(
            new Dictionary<string, GlobalHeadcountCheckpoint>(),
            new List<string>());
    }

    public static IGlobalHeadcountCheckpointStore NewCachedGlobalHeadcountCheckpointStore(MemoryGlobalHeadcountCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalHeadcountCheckpointStore(
            inner,
            GlobalHeadcountCheckpointCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalHeadcountCheckpoint> FindGlobalHeadcountCheckpoints(IGlobalHeadcountCheckpointStore store, GlobalHeadcountCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
