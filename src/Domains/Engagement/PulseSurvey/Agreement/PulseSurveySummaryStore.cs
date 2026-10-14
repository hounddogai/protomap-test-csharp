namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pulse survey summaries by code.</summary>
public interface IPulseSurveySummaryStore
{
    PulseSurveySummary? Find(string code);
    void Save(PulseSurveySummary item);
    bool Remove(string code);
    List<PulseSurveySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPulseSurveySummaryStore(Dictionary<string, PulseSurveySummary> items, List<string> order) : IPulseSurveySummaryStore
{
    public Dictionary<string, PulseSurveySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PulseSurveySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PulseSurveySummary item)
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

    public List<PulseSurveySummary> List()
    {
        var outValue = new List<PulseSurveySummary>();
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
public sealed class CachedPulseSurveySummaryStore(MemoryPulseSurveySummaryStore inner, PulseSurveySummaryCache cache, int hits, int misses) : IPulseSurveySummaryStore
{
    public MemoryPulseSurveySummaryStore Inner { get; set; } = inner;
    public PulseSurveySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PulseSurveySummary? Find(string code)
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

    public void Save(PulseSurveySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PulseSurveySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pulse survey summaries. Empty fields match all.</summary>
public sealed record PulseSurveySummaryQuery(string Name, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PulseSurveySummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PulseSurveySummary> Apply(List<PulseSurveySummary> items)
    {
        var matched = Collections.Filter(items, (PulseSurveySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pulse survey summary store.</summary>
public static class PulseSurveySummaryStore
{
    public static IPulseSurveySummaryStore NewMemoryPulseSurveySummaryStore()
    {
        return new MemoryPulseSurveySummaryStore(new Dictionary<string, PulseSurveySummary>(), new List<string>());
    }

    public static IPulseSurveySummaryStore NewCachedPulseSurveySummaryStore(MemoryPulseSurveySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPulseSurveySummaryStore(inner, PulseSurveySummaryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PulseSurveySummary> FindPulseSurveySummaries(IPulseSurveySummaryStore store, PulseSurveySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
