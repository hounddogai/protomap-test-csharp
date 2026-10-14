namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant summaries by code.</summary>
public interface IEquityGrantSummaryStore
{
    EquityGrantSummary? Find(string code);
    void Save(EquityGrantSummary item);
    bool Remove(string code);
    List<EquityGrantSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantSummaryStore(Dictionary<string, EquityGrantSummary> items, List<string> order) : IEquityGrantSummaryStore
{
    public Dictionary<string, EquityGrantSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantSummary item)
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

    public List<EquityGrantSummary> List()
    {
        var outValue = new List<EquityGrantSummary>();
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
public sealed class CachedEquityGrantSummaryStore(MemoryEquityGrantSummaryStore inner, EquityGrantSummaryCache cache, int hits, int misses) : IEquityGrantSummaryStore
{
    public MemoryEquityGrantSummaryStore Inner { get; set; } = inner;
    public EquityGrantSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantSummary? Find(string code)
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

    public void Save(EquityGrantSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant summaries. Empty fields match all.</summary>
public sealed record EquityGrantSummaryQuery(string Unit, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantSummary item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
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

    public List<EquityGrantSummary> Apply(List<EquityGrantSummary> items)
    {
        var matched = Collections.Filter(items, (EquityGrantSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant summary store.</summary>
public static class EquityGrantSummaryStore
{
    public static IEquityGrantSummaryStore NewMemoryEquityGrantSummaryStore()
    {
        return new MemoryEquityGrantSummaryStore(new Dictionary<string, EquityGrantSummary>(), new List<string>());
    }

    public static IEquityGrantSummaryStore NewCachedEquityGrantSummaryStore(MemoryEquityGrantSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantSummaryStore(inner, EquityGrantSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantSummary> FindEquityGrantSummaries(IEquityGrantSummaryStore store, EquityGrantSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
