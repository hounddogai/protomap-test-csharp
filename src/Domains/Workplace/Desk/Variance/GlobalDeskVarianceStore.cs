namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global desk variances by code.</summary>
public interface IGlobalDeskVarianceStore
{
    GlobalDeskVariance? Find(string code);
    void Save(GlobalDeskVariance item);
    bool Remove(string code);
    List<GlobalDeskVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalDeskVarianceStore(Dictionary<string, GlobalDeskVariance> items, List<string> order) : IGlobalDeskVarianceStore
{
    public Dictionary<string, GlobalDeskVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalDeskVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalDeskVariance item)
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

    public List<GlobalDeskVariance> List()
    {
        var outValue = new List<GlobalDeskVariance>();
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
public sealed class CachedGlobalDeskVarianceStore(MemoryGlobalDeskVarianceStore inner, GlobalDeskVarianceCache cache, int hits, int misses) : IGlobalDeskVarianceStore
{
    public MemoryGlobalDeskVarianceStore Inner { get; set; } = inner;
    public GlobalDeskVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalDeskVariance? Find(string code)
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

    public void Save(GlobalDeskVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalDeskVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global desk variances. Empty fields match all.</summary>
public sealed record GlobalDeskVarianceQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalDeskVariance item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<GlobalDeskVariance> Apply(List<GlobalDeskVariance> items)
    {
        var matched = Collections.Filter(items, (GlobalDeskVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global desk variance store.</summary>
public static class GlobalDeskVarianceStore
{
    public static IGlobalDeskVarianceStore NewMemoryGlobalDeskVarianceStore()
    {
        return new MemoryGlobalDeskVarianceStore(new Dictionary<string, GlobalDeskVariance>(), new List<string>());
    }

    public static IGlobalDeskVarianceStore NewCachedGlobalDeskVarianceStore(MemoryGlobalDeskVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalDeskVarianceStore(inner, GlobalDeskVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GlobalDeskVariance> FindGlobalDeskVariances(IGlobalDeskVarianceStore store, GlobalDeskVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
