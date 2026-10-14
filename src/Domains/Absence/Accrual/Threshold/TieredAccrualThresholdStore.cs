namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered accrual thresholds by code.</summary>
public interface ITieredAccrualThresholdStore
{
    TieredAccrualThreshold? Find(string code);
    void Save(TieredAccrualThreshold item);
    bool Remove(string code);
    List<TieredAccrualThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredAccrualThresholdStore(Dictionary<string, TieredAccrualThreshold> items, List<string> order) : ITieredAccrualThresholdStore
{
    public Dictionary<string, TieredAccrualThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredAccrualThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredAccrualThreshold item)
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

    public List<TieredAccrualThreshold> List()
    {
        var outValue = new List<TieredAccrualThreshold>();
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
public sealed class CachedTieredAccrualThresholdStore(MemoryTieredAccrualThresholdStore inner, TieredAccrualThresholdCache cache, int hits, int misses) : ITieredAccrualThresholdStore
{
    public MemoryTieredAccrualThresholdStore Inner { get; set; } = inner;
    public TieredAccrualThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredAccrualThreshold? Find(string code)
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

    public void Save(TieredAccrualThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredAccrualThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered accrual thresholds. Empty fields match all.</summary>
public sealed record TieredAccrualThresholdQuery(string Segment, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(TieredAccrualThreshold item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredAccrualThreshold> Apply(List<TieredAccrualThreshold> items)
    {
        var matched = Collections.Filter(items, (TieredAccrualThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered accrual threshold store.</summary>
public static class TieredAccrualThresholdStore
{
    public static ITieredAccrualThresholdStore NewMemoryTieredAccrualThresholdStore()
    {
        return new MemoryTieredAccrualThresholdStore(
            new Dictionary<string, TieredAccrualThreshold>(),
            new List<string>());
    }

    public static ITieredAccrualThresholdStore NewCachedTieredAccrualThresholdStore(MemoryTieredAccrualThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedTieredAccrualThresholdStore(
            inner,
            TieredAccrualThresholdCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<TieredAccrualThreshold> FindTieredAccrualThresholds(ITieredAccrualThresholdStore store, TieredAccrualThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
