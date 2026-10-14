namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling retro pay summaries by code.</summary>
public interface IRollingRetroPaySummaryStore
{
    RollingRetroPaySummary? Find(string code);
    void Save(RollingRetroPaySummary item);
    bool Remove(string code);
    List<RollingRetroPaySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingRetroPaySummaryStore(Dictionary<string, RollingRetroPaySummary> items, List<string> order) : IRollingRetroPaySummaryStore
{
    public Dictionary<string, RollingRetroPaySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingRetroPaySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingRetroPaySummary item)
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

    public List<RollingRetroPaySummary> List()
    {
        var outValue = new List<RollingRetroPaySummary>();
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
public sealed class CachedRollingRetroPaySummaryStore(MemoryRollingRetroPaySummaryStore inner, RollingRetroPaySummaryCache cache, int hits, int misses) : IRollingRetroPaySummaryStore
{
    public MemoryRollingRetroPaySummaryStore Inner { get; set; } = inner;
    public RollingRetroPaySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingRetroPaySummary? Find(string code)
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

    public void Save(RollingRetroPaySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingRetroPaySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling retro pay summaries. Empty fields match all.</summary>
public sealed record RollingRetroPaySummaryQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(RollingRetroPaySummary item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<RollingRetroPaySummary> Apply(List<RollingRetroPaySummary> items)
    {
        var matched = Collections.Filter(items, (RollingRetroPaySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling retro pay summary store.</summary>
public static class RollingRetroPaySummaryStore
{
    public static IRollingRetroPaySummaryStore NewMemoryRollingRetroPaySummaryStore()
    {
        return new MemoryRollingRetroPaySummaryStore(
            new Dictionary<string, RollingRetroPaySummary>(),
            new List<string>());
    }

    public static IRollingRetroPaySummaryStore NewCachedRollingRetroPaySummaryStore(MemoryRollingRetroPaySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedRollingRetroPaySummaryStore(
            inner,
            RollingRetroPaySummaryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingRetroPaySummary> FindRollingRetroPaySummaries(IRollingRetroPaySummaryStore store, RollingRetroPaySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
