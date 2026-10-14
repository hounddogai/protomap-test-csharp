namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band summaries by code.</summary>
public interface IPayBandSummaryStore
{
    PayBandSummary? Find(string code);
    void Save(PayBandSummary item);
    bool Remove(string code);
    List<PayBandSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandSummaryStore(Dictionary<string, PayBandSummary> items, List<string> order) : IPayBandSummaryStore
{
    public Dictionary<string, PayBandSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandSummary item)
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

    public List<PayBandSummary> List()
    {
        var outValue = new List<PayBandSummary>();
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
public sealed class CachedPayBandSummaryStore(MemoryPayBandSummaryStore inner, PayBandSummaryCache cache, int hits, int misses) : IPayBandSummaryStore
{
    public MemoryPayBandSummaryStore Inner { get; set; } = inner;
    public PayBandSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandSummary? Find(string code)
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

    public void Save(PayBandSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band summaries. Empty fields match all.</summary>
public sealed record PayBandSummaryQuery(string Category, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandSummary item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandSummary> Apply(List<PayBandSummary> items)
    {
        var matched = Collections.Filter(items, (PayBandSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band summary store.</summary>
public static class PayBandSummaryStore
{
    public static IPayBandSummaryStore NewMemoryPayBandSummaryStore()
    {
        return new MemoryPayBandSummaryStore(new Dictionary<string, PayBandSummary>(), new List<string>());
    }

    public static IPayBandSummaryStore NewCachedPayBandSummaryStore(MemoryPayBandSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandSummaryStore(inner, PayBandSummaryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayBandSummary> FindPayBandSummaries(IPayBandSummaryStore store, PayBandSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
