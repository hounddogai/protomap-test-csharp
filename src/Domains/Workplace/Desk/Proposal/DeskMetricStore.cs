namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps desk metrics by code.</summary>
public interface IDeskMetricStore
{
    DeskMetric? Find(string code);
    void Save(DeskMetric item);
    bool Remove(string code);
    List<DeskMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDeskMetricStore(Dictionary<string, DeskMetric> items, List<string> order) : IDeskMetricStore
{
    public Dictionary<string, DeskMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DeskMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DeskMetric item)
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

    public List<DeskMetric> List()
    {
        var outValue = new List<DeskMetric>();
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
public sealed class CachedDeskMetricStore(MemoryDeskMetricStore inner, DeskMetricCache cache, int hits, int misses) : IDeskMetricStore
{
    public MemoryDeskMetricStore Inner { get; set; } = inner;
    public DeskMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DeskMetric? Find(string code)
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

    public void Save(DeskMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DeskMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects desk metrics. Empty fields match all.</summary>
public sealed record DeskMetricQuery(string Name, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(DeskMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DeskMetric> Apply(List<DeskMetric> items)
    {
        var matched = Collections.Filter(items, (DeskMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of desk metric store.</summary>
public static class DeskMetricStore
{
    public static IDeskMetricStore NewMemoryDeskMetricStore()
    {
        return new MemoryDeskMetricStore(new Dictionary<string, DeskMetric>(), new List<string>());
    }

    public static IDeskMetricStore NewCachedDeskMetricStore(MemoryDeskMetricStore inner, int ttl, IClock clock)
    {
        return new CachedDeskMetricStore(inner, DeskMetricCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DeskMetric> FindDeskMetrics(IDeskMetricStore store, DeskMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
