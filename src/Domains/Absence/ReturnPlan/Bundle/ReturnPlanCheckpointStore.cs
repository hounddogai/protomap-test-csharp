namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan checkpoints by code.</summary>
public interface IReturnPlanCheckpointStore
{
    ReturnPlanCheckpoint? Find(string code);
    void Save(ReturnPlanCheckpoint item);
    bool Remove(string code);
    List<ReturnPlanCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanCheckpointStore(Dictionary<string, ReturnPlanCheckpoint> items, List<string> order) : IReturnPlanCheckpointStore
{
    public Dictionary<string, ReturnPlanCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanCheckpoint item)
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

    public List<ReturnPlanCheckpoint> List()
    {
        var outValue = new List<ReturnPlanCheckpoint>();
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
public sealed class CachedReturnPlanCheckpointStore(MemoryReturnPlanCheckpointStore inner, ReturnPlanCheckpointCache cache, int hits, int misses) : IReturnPlanCheckpointStore
{
    public MemoryReturnPlanCheckpointStore Inner { get; set; } = inner;
    public ReturnPlanCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanCheckpoint? Find(string code)
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

    public void Save(ReturnPlanCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan checkpoints. Empty fields match all.</summary>
public sealed record ReturnPlanCheckpointQuery(string Title, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanCheckpoint item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanCheckpoint> Apply(List<ReturnPlanCheckpoint> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan checkpoint store.</summary>
public static class ReturnPlanCheckpointStore
{
    public static IReturnPlanCheckpointStore NewMemoryReturnPlanCheckpointStore()
    {
        return new MemoryReturnPlanCheckpointStore(
            new Dictionary<string, ReturnPlanCheckpoint>(),
            new List<string>());
    }

    public static IReturnPlanCheckpointStore NewCachedReturnPlanCheckpointStore(MemoryReturnPlanCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanCheckpointStore(inner, ReturnPlanCheckpointCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanCheckpoint> FindReturnPlanCheckpoints(IReturnPlanCheckpointStore store, ReturnPlanCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
