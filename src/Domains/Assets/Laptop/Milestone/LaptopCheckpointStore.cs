namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps laptop checkpoints by code.</summary>
public interface ILaptopCheckpointStore
{
    LaptopCheckpoint? Find(string code);
    void Save(LaptopCheckpoint item);
    bool Remove(string code);
    List<LaptopCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLaptopCheckpointStore(Dictionary<string, LaptopCheckpoint> items, List<string> order) : ILaptopCheckpointStore
{
    public Dictionary<string, LaptopCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LaptopCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LaptopCheckpoint item)
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

    public List<LaptopCheckpoint> List()
    {
        var outValue = new List<LaptopCheckpoint>();
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
public sealed class CachedLaptopCheckpointStore(MemoryLaptopCheckpointStore inner, LaptopCheckpointCache cache, int hits, int misses) : ILaptopCheckpointStore
{
    public MemoryLaptopCheckpointStore Inner { get; set; } = inner;
    public LaptopCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LaptopCheckpoint? Find(string code)
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

    public void Save(LaptopCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LaptopCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects laptop checkpoints. Empty fields match all.</summary>
public sealed record LaptopCheckpointQuery(string Label, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(LaptopCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LaptopCheckpoint> Apply(List<LaptopCheckpoint> items)
    {
        var matched = Collections.Filter(items, (LaptopCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of laptop checkpoint store.</summary>
public static class LaptopCheckpointStore
{
    public static ILaptopCheckpointStore NewMemoryLaptopCheckpointStore()
    {
        return new MemoryLaptopCheckpointStore(new Dictionary<string, LaptopCheckpoint>(), new List<string>());
    }

    public static ILaptopCheckpointStore NewCachedLaptopCheckpointStore(MemoryLaptopCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedLaptopCheckpointStore(inner, LaptopCheckpointCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LaptopCheckpoint> FindLaptopCheckpoints(ILaptopCheckpointStore store, LaptopCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
