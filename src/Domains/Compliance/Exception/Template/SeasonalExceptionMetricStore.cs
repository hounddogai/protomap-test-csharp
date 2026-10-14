namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal exception metrics by code.</summary>
public interface ISeasonalExceptionMetricStore
{
    SeasonalExceptionMetric? Find(string code);
    void Save(SeasonalExceptionMetric item);
    bool Remove(string code);
    List<SeasonalExceptionMetric> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalExceptionMetricStore(Dictionary<string, SeasonalExceptionMetric> items, List<string> order) : ISeasonalExceptionMetricStore
{
    public Dictionary<string, SeasonalExceptionMetric> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalExceptionMetric? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalExceptionMetric item)
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

    public List<SeasonalExceptionMetric> List()
    {
        var outValue = new List<SeasonalExceptionMetric>();
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
public sealed class CachedSeasonalExceptionMetricStore(MemorySeasonalExceptionMetricStore inner, SeasonalExceptionMetricCache cache, int hits, int misses) : ISeasonalExceptionMetricStore
{
    public MemorySeasonalExceptionMetricStore Inner { get; set; } = inner;
    public SeasonalExceptionMetricCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalExceptionMetric? Find(string code)
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

    public void Save(SeasonalExceptionMetric item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalExceptionMetric> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal exception metrics. Empty fields match all.</summary>
public sealed record SeasonalExceptionMetricQuery(string Name, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalExceptionMetric item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalExceptionMetric> Apply(List<SeasonalExceptionMetric> items)
    {
        var matched = Collections.Filter(items, (SeasonalExceptionMetric candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal exception metric store.</summary>
public static class SeasonalExceptionMetricStore
{
    public static ISeasonalExceptionMetricStore NewMemorySeasonalExceptionMetricStore()
    {
        return new MemorySeasonalExceptionMetricStore(
            new Dictionary<string, SeasonalExceptionMetric>(),
            new List<string>());
    }

    public static ISeasonalExceptionMetricStore NewCachedSeasonalExceptionMetricStore(MemorySeasonalExceptionMetricStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalExceptionMetricStore(
            inner,
            SeasonalExceptionMetricCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalExceptionMetric> FindSeasonalExceptionMetrics(ISeasonalExceptionMetricStore store, SeasonalExceptionMetricQuery query)
    {
        return query.Apply(store.List());
    }
}
