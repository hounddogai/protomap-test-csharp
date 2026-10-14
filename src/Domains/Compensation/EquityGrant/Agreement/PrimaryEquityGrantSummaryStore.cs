namespace Acme.Workforce.Compensation.EquityGrant.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary equity grant summaries by code.</summary>
public interface IPrimaryEquityGrantSummaryStore
{
    PrimaryEquityGrantSummary? Find(string code);
    void Save(PrimaryEquityGrantSummary item);
    bool Remove(string code);
    List<PrimaryEquityGrantSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryEquityGrantSummaryStore(Dictionary<string, PrimaryEquityGrantSummary> items, List<string> order) : IPrimaryEquityGrantSummaryStore
{
    public Dictionary<string, PrimaryEquityGrantSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryEquityGrantSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryEquityGrantSummary item)
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

    public List<PrimaryEquityGrantSummary> List()
    {
        var outValue = new List<PrimaryEquityGrantSummary>();
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
public sealed class CachedPrimaryEquityGrantSummaryStore(MemoryPrimaryEquityGrantSummaryStore inner, PrimaryEquityGrantSummaryCache cache, int hits, int misses) : IPrimaryEquityGrantSummaryStore
{
    public MemoryPrimaryEquityGrantSummaryStore Inner { get; set; } = inner;
    public PrimaryEquityGrantSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryEquityGrantSummary? Find(string code)
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

    public void Save(PrimaryEquityGrantSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryEquityGrantSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary equity grant summaries. Empty fields match all.</summary>
public sealed record PrimaryEquityGrantSummaryQuery(string Channel, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryEquityGrantSummary item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryEquityGrantSummary> Apply(List<PrimaryEquityGrantSummary> items)
    {
        var matched = Collections.Filter(items, (PrimaryEquityGrantSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary equity grant summary store.</summary>
public static class PrimaryEquityGrantSummaryStore
{
    public static IPrimaryEquityGrantSummaryStore NewMemoryPrimaryEquityGrantSummaryStore()
    {
        return new MemoryPrimaryEquityGrantSummaryStore(
            new Dictionary<string, PrimaryEquityGrantSummary>(),
            new List<string>());
    }

    public static IPrimaryEquityGrantSummaryStore NewCachedPrimaryEquityGrantSummaryStore(MemoryPrimaryEquityGrantSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryEquityGrantSummaryStore(
            inner,
            PrimaryEquityGrantSummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryEquityGrantSummary> FindPrimaryEquityGrantSummaries(IPrimaryEquityGrantSummaryStore store, PrimaryEquityGrantSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
