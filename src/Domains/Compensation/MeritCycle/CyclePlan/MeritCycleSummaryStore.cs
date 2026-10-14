namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle summaries by code.</summary>
public interface IMeritCycleSummaryStore
{
    MeritCycleSummary? Find(string code);
    void Save(MeritCycleSummary item);
    bool Remove(string code);
    List<MeritCycleSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleSummaryStore(Dictionary<string, MeritCycleSummary> items, List<string> order) : IMeritCycleSummaryStore
{
    public Dictionary<string, MeritCycleSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleSummary item)
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

    public List<MeritCycleSummary> List()
    {
        var outValue = new List<MeritCycleSummary>();
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
public sealed class CachedMeritCycleSummaryStore(MemoryMeritCycleSummaryStore inner, MeritCycleSummaryCache cache, int hits, int misses) : IMeritCycleSummaryStore
{
    public MemoryMeritCycleSummaryStore Inner { get; set; } = inner;
    public MeritCycleSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleSummary? Find(string code)
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

    public void Save(MeritCycleSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle summaries. Empty fields match all.</summary>
public sealed record MeritCycleSummaryQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleSummary item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleSummary> Apply(List<MeritCycleSummary> items)
    {
        var matched = Collections.Filter(items, (MeritCycleSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle summary store.</summary>
public static class MeritCycleSummaryStore
{
    public static IMeritCycleSummaryStore NewMemoryMeritCycleSummaryStore()
    {
        return new MemoryMeritCycleSummaryStore(new Dictionary<string, MeritCycleSummary>(), new List<string>());
    }

    public static IMeritCycleSummaryStore NewCachedMeritCycleSummaryStore(MemoryMeritCycleSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleSummaryStore(inner, MeritCycleSummaryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MeritCycleSummary> FindMeritCycleSummaries(IMeritCycleSummaryStore store, MeritCycleSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
