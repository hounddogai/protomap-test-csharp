namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered headcount estimates by code.</summary>
public interface ITieredHeadcountEstimateStore
{
    TieredHeadcountEstimate? Find(string code);
    void Save(TieredHeadcountEstimate item);
    bool Remove(string code);
    List<TieredHeadcountEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredHeadcountEstimateStore(Dictionary<string, TieredHeadcountEstimate> items, List<string> order) : ITieredHeadcountEstimateStore
{
    public Dictionary<string, TieredHeadcountEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredHeadcountEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredHeadcountEstimate item)
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

    public List<TieredHeadcountEstimate> List()
    {
        var outValue = new List<TieredHeadcountEstimate>();
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
public sealed class CachedTieredHeadcountEstimateStore(MemoryTieredHeadcountEstimateStore inner, TieredHeadcountEstimateCache cache, int hits, int misses) : ITieredHeadcountEstimateStore
{
    public MemoryTieredHeadcountEstimateStore Inner { get; set; } = inner;
    public TieredHeadcountEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredHeadcountEstimate? Find(string code)
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

    public void Save(TieredHeadcountEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredHeadcountEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered headcount estimates. Empty fields match all.</summary>
public sealed record TieredHeadcountEstimateQuery(string Source, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(TieredHeadcountEstimate item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredHeadcountEstimate> Apply(List<TieredHeadcountEstimate> items)
    {
        var matched = Collections.Filter(items, (TieredHeadcountEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered headcount estimate store.</summary>
public static class TieredHeadcountEstimateStore
{
    public static ITieredHeadcountEstimateStore NewMemoryTieredHeadcountEstimateStore()
    {
        return new MemoryTieredHeadcountEstimateStore(
            new Dictionary<string, TieredHeadcountEstimate>(),
            new List<string>());
    }

    public static ITieredHeadcountEstimateStore NewCachedTieredHeadcountEstimateStore(MemoryTieredHeadcountEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedTieredHeadcountEstimateStore(
            inner,
            TieredHeadcountEstimateCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredHeadcountEstimate> FindTieredHeadcountEstimates(ITieredHeadcountEstimateStore store, TieredHeadcountEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
