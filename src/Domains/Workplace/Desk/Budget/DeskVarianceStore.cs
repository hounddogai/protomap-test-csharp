namespace Acme.Workforce.Workplace.Desk.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk variances by code.</summary>
public interface IDeskVarianceStore
{
    DeskVariance? Find(string code);
    void Save(DeskVariance item);
    bool Remove(string code);
    List<DeskVariance> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskVarianceStore(Dictionary<string, DeskVariance> items, List<string> order) : IDeskVarianceStore
{
    public Dictionary<string, DeskVariance> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskVariance? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskVariance item)
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

    public List<DeskVariance> List()
    {
        var outValue = new List<DeskVariance>();
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
public sealed class CachedDeskVarianceStore(MemoryDeskVarianceStore inner, DeskVarianceCache cache, int hits, int misses) : IDeskVarianceStore
{
    public MemoryDeskVarianceStore Inner { get; set; } = inner;
    public DeskVarianceCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskVariance? Find(string code)
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

    public void Save(DeskVariance item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskVariance> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk variances. Empty fields match all.</summary>
public sealed record DeskVarianceQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(DeskVariance item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskVariance> Apply(List<DeskVariance> items)
    {
        var matched = Collections.Filter(items, (DeskVariance candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk variance store.</summary>
public static class DeskVarianceStore
{
    public static IDeskVarianceStore NewMemoryDeskVarianceStore()
    {
        return new MemoryDeskVarianceStore(new Dictionary<string, DeskVariance>(), new List<string>());
    }

    public static IDeskVarianceStore NewCachedDeskVarianceStore(MemoryDeskVarianceStore inner, int ttl, IClock clock)
    {
        return new CachedDeskVarianceStore(inner, DeskVarianceCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeskVariance> FindDeskVariances(IDeskVarianceStore store, DeskVarianceQuery query)
    {
        return query.Apply(store.List());
    }
}
