namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict recognition cycles by code.</summary>
public interface IStrictRecognitionCycleStore
{
    StrictRecognitionCycle? Find(string code);
    void Save(StrictRecognitionCycle item);
    bool Remove(string code);
    List<StrictRecognitionCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictRecognitionCycleStore(Dictionary<string, StrictRecognitionCycle> items, List<string> order) : IStrictRecognitionCycleStore
{
    public Dictionary<string, StrictRecognitionCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictRecognitionCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictRecognitionCycle item)
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

    public List<StrictRecognitionCycle> List()
    {
        var outValue = new List<StrictRecognitionCycle>();
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
public sealed class CachedStrictRecognitionCycleStore(MemoryStrictRecognitionCycleStore inner, StrictRecognitionCycleCache cache, int hits, int misses) : IStrictRecognitionCycleStore
{
    public MemoryStrictRecognitionCycleStore Inner { get; set; } = inner;
    public StrictRecognitionCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictRecognitionCycle? Find(string code)
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

    public void Save(StrictRecognitionCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictRecognitionCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict recognition cycles. Empty fields match all.</summary>
public sealed record StrictRecognitionCycleQuery(string Name, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(StrictRecognitionCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictRecognitionCycle> Apply(List<StrictRecognitionCycle> items)
    {
        var matched = Collections.Filter(items, (StrictRecognitionCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict recognition cycle store.</summary>
public static class StrictRecognitionCycleStore
{
    public static IStrictRecognitionCycleStore NewMemoryStrictRecognitionCycleStore()
    {
        return new MemoryStrictRecognitionCycleStore(
            new Dictionary<string, StrictRecognitionCycle>(),
            new List<string>());
    }

    public static IStrictRecognitionCycleStore NewCachedStrictRecognitionCycleStore(MemoryStrictRecognitionCycleStore inner, int ttl, IClock clock)
    {
        return new CachedStrictRecognitionCycleStore(
            inner,
            StrictRecognitionCycleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<StrictRecognitionCycle> FindStrictRecognitionCycles(IStrictRecognitionCycleStore store, StrictRecognitionCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
