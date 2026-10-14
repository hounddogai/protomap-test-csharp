namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy garnishment metrics by code.</summary>
public interface ILegacyGarnishmentMetricStore
{
    LegacyGarnishmentMetric? Find(string code);
    void Save(LegacyGarnishmentMetric item);
    bool Remove(string code);
    List<LegacyGarnishmentMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyGarnishmentMetricStore(Dictionary<string, LegacyGarnishmentMetric> items, List<string> order) : ILegacyGarnishmentMetricStore
{
    public Dictionary<string, LegacyGarnishmentMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyGarnishmentMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyGarnishmentMetric item)
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

    public List<LegacyGarnishmentMetric> List()
    {
        var outValue = new List<LegacyGarnishmentMetric>();
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
public sealed class CachedLegacyGarnishmentMetricStore(MemoryLegacyGarnishmentMetricStore inner, LegacyGarnishmentMetricCache cache, int hits, int misses) : ILegacyGarnishmentMetricStore
{
    public MemoryLegacyGarnishmentMetricStore Inner { get; set; } = inner;
    public LegacyGarnishmentMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyGarnishmentMetric? Find(string code)
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

    public void Save(LegacyGarnishmentMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyGarnishmentMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy garnishment metrics. Empty fields match all.</summary>
public sealed record LegacyGarnishmentMetricQuery(string Label, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyGarnishmentMetric item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyGarnishmentMetric> Apply(List<LegacyGarnishmentMetric> items)
    {
        var matched = Collections.Filter(items, (LegacyGarnishmentMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy garnishment metric store.</summary>
public static class LegacyGarnishmentMetricStore
{
    public static ILegacyGarnishmentMetricStore NewMemoryLegacyGarnishmentMetricStore()
    {
        return new MemoryLegacyGarnishmentMetricStore(
            new Dictionary<string, LegacyGarnishmentMetric>(),
            new List<string>());
    }

    public static ILegacyGarnishmentMetricStore NewCachedLegacyGarnishmentMetricStore(MemoryLegacyGarnishmentMetricStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyGarnishmentMetricStore(
            inner,
            LegacyGarnishmentMetricCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyGarnishmentMetric> FindLegacyGarnishmentMetrics(ILegacyGarnishmentMetricStore store, LegacyGarnishmentMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
