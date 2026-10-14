namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps fallback succession plan entries by code.</summary>
public interface IFallbackSuccessionPlanEntryStore
{
    FallbackSuccessionPlanEntry? Find(string code);
    void Save(FallbackSuccessionPlanEntry item);
    bool Remove(string code);
    List<FallbackSuccessionPlanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFallbackSuccessionPlanEntryStore(Dictionary<string, FallbackSuccessionPlanEntry> items, List<string> order) : IFallbackSuccessionPlanEntryStore
{
    public Dictionary<string, FallbackSuccessionPlanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FallbackSuccessionPlanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FallbackSuccessionPlanEntry item)
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

    public List<FallbackSuccessionPlanEntry> List()
    {
        var outValue = new List<FallbackSuccessionPlanEntry>();
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
public sealed class CachedFallbackSuccessionPlanEntryStore(MemoryFallbackSuccessionPlanEntryStore inner, FallbackSuccessionPlanEntryCache cache, int hits, int misses) : IFallbackSuccessionPlanEntryStore
{
    public MemoryFallbackSuccessionPlanEntryStore Inner { get; set; } = inner;
    public FallbackSuccessionPlanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FallbackSuccessionPlanEntry? Find(string code)
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

    public void Save(FallbackSuccessionPlanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FallbackSuccessionPlanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects fallback succession plan entries. Empty fields match all.</summary>
public sealed record FallbackSuccessionPlanEntryQuery(string Locale, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(FallbackSuccessionPlanEntry item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FallbackSuccessionPlanEntry> Apply(List<FallbackSuccessionPlanEntry> items)
    {
        var matched = Collections.Filter(items, (FallbackSuccessionPlanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of fallback succession plan entry store.</summary>
public static class FallbackSuccessionPlanEntryStore
{
    public static IFallbackSuccessionPlanEntryStore NewMemoryFallbackSuccessionPlanEntryStore()
    {
        return new MemoryFallbackSuccessionPlanEntryStore(
            new Dictionary<string, FallbackSuccessionPlanEntry>(),
            new List<string>());
    }

    public static IFallbackSuccessionPlanEntryStore NewCachedFallbackSuccessionPlanEntryStore(MemoryFallbackSuccessionPlanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedFallbackSuccessionPlanEntryStore(
            inner,
            FallbackSuccessionPlanEntryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<FallbackSuccessionPlanEntry> FindFallbackSuccessionPlanEntries(IFallbackSuccessionPlanEntryStore store, FallbackSuccessionPlanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
