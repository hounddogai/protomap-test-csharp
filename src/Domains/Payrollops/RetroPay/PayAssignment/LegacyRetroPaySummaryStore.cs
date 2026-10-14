namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy retro pay summaries by code.</summary>
public interface ILegacyRetroPaySummaryStore
{
    LegacyRetroPaySummary? Find(string code);
    void Save(LegacyRetroPaySummary item);
    bool Remove(string code);
    List<LegacyRetroPaySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyRetroPaySummaryStore(Dictionary<string, LegacyRetroPaySummary> items, List<string> order) : ILegacyRetroPaySummaryStore
{
    public Dictionary<string, LegacyRetroPaySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyRetroPaySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyRetroPaySummary item)
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

    public List<LegacyRetroPaySummary> List()
    {
        var outValue = new List<LegacyRetroPaySummary>();
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
public sealed class CachedLegacyRetroPaySummaryStore(MemoryLegacyRetroPaySummaryStore inner, LegacyRetroPaySummaryCache cache, int hits, int misses) : ILegacyRetroPaySummaryStore
{
    public MemoryLegacyRetroPaySummaryStore Inner { get; set; } = inner;
    public LegacyRetroPaySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyRetroPaySummary? Find(string code)
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

    public void Save(LegacyRetroPaySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyRetroPaySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy retro pay summaries. Empty fields match all.</summary>
public sealed record LegacyRetroPaySummaryQuery(string Region, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyRetroPaySummary item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyRetroPaySummary> Apply(List<LegacyRetroPaySummary> items)
    {
        var matched = Collections.Filter(items, (LegacyRetroPaySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy retro pay summary store.</summary>
public static class LegacyRetroPaySummaryStore
{
    public static ILegacyRetroPaySummaryStore NewMemoryLegacyRetroPaySummaryStore()
    {
        return new MemoryLegacyRetroPaySummaryStore(
            new Dictionary<string, LegacyRetroPaySummary>(),
            new List<string>());
    }

    public static ILegacyRetroPaySummaryStore NewCachedLegacyRetroPaySummaryStore(MemoryLegacyRetroPaySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyRetroPaySummaryStore(
            inner,
            LegacyRetroPaySummaryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyRetroPaySummary> FindLegacyRetroPaySummaries(ILegacyRetroPaySummaryStore store, LegacyRetroPaySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
