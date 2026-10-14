namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual pulse survey summaries by code.</summary>
public interface IManualPulseSurveySummaryStore
{
    ManualPulseSurveySummary? Find(string code);
    void Save(ManualPulseSurveySummary item);
    bool Remove(string code);
    List<ManualPulseSurveySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPulseSurveySummaryStore(Dictionary<string, ManualPulseSurveySummary> items, List<string> order) : IManualPulseSurveySummaryStore
{
    public Dictionary<string, ManualPulseSurveySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPulseSurveySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPulseSurveySummary item)
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

    public List<ManualPulseSurveySummary> List()
    {
        var outValue = new List<ManualPulseSurveySummary>();
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
public sealed class CachedManualPulseSurveySummaryStore(MemoryManualPulseSurveySummaryStore inner, ManualPulseSurveySummaryCache cache, int hits, int misses) : IManualPulseSurveySummaryStore
{
    public MemoryManualPulseSurveySummaryStore Inner { get; set; } = inner;
    public ManualPulseSurveySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPulseSurveySummary? Find(string code)
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

    public void Save(ManualPulseSurveySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPulseSurveySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual pulse survey summaries. Empty fields match all.</summary>
public sealed record ManualPulseSurveySummaryQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPulseSurveySummary item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPulseSurveySummary> Apply(List<ManualPulseSurveySummary> items)
    {
        var matched = Collections.Filter(items, (ManualPulseSurveySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual pulse survey summary store.</summary>
public static class ManualPulseSurveySummaryStore
{
    public static IManualPulseSurveySummaryStore NewMemoryManualPulseSurveySummaryStore()
    {
        return new MemoryManualPulseSurveySummaryStore(
            new Dictionary<string, ManualPulseSurveySummary>(),
            new List<string>());
    }

    public static IManualPulseSurveySummaryStore NewCachedManualPulseSurveySummaryStore(MemoryManualPulseSurveySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedManualPulseSurveySummaryStore(
            inner,
            ManualPulseSurveySummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<ManualPulseSurveySummary> FindManualPulseSurveySummaries(IManualPulseSurveySummaryStore store, ManualPulseSurveySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
