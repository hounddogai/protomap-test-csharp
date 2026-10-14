namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback training due rollups by code.</summary>
public interface IFallbackTrainingDueRollupStore
{
    FallbackTrainingDueRollup? Find(string code);
    void Save(FallbackTrainingDueRollup item);
    bool Remove(string code);
    List<FallbackTrainingDueRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackTrainingDueRollupStore(Dictionary<string, FallbackTrainingDueRollup> items, List<string> order) : IFallbackTrainingDueRollupStore
{
    public Dictionary<string, FallbackTrainingDueRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackTrainingDueRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackTrainingDueRollup item)
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

    public List<FallbackTrainingDueRollup> List()
    {
        var outValue = new List<FallbackTrainingDueRollup>();
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
public sealed class CachedFallbackTrainingDueRollupStore(MemoryFallbackTrainingDueRollupStore inner, FallbackTrainingDueRollupCache cache, int hits, int misses) : IFallbackTrainingDueRollupStore
{
    public MemoryFallbackTrainingDueRollupStore Inner { get; set; } = inner;
    public FallbackTrainingDueRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackTrainingDueRollup? Find(string code)
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

    public void Save(FallbackTrainingDueRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackTrainingDueRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback training due rollups. Empty fields match all.</summary>
public sealed record FallbackTrainingDueRollupQuery(string Source, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackTrainingDueRollup item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackTrainingDueRollup> Apply(List<FallbackTrainingDueRollup> items)
    {
        var matched = Collections.Filter(items, (FallbackTrainingDueRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback training due rollup store.</summary>
public static class FallbackTrainingDueRollupStore
{
    public static IFallbackTrainingDueRollupStore NewMemoryFallbackTrainingDueRollupStore()
    {
        return new MemoryFallbackTrainingDueRollupStore(
            new Dictionary<string, FallbackTrainingDueRollup>(),
            new List<string>());
    }

    public static IFallbackTrainingDueRollupStore NewCachedFallbackTrainingDueRollupStore(MemoryFallbackTrainingDueRollupStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackTrainingDueRollupStore(
            inner,
            FallbackTrainingDueRollupCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<FallbackTrainingDueRollup> FindFallbackTrainingDueRollups(IFallbackTrainingDueRollupStore store, FallbackTrainingDueRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
