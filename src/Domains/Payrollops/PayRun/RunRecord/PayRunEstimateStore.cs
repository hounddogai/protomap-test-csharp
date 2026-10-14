namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run estimates by code.</summary>
public interface IPayRunEstimateStore
{
    PayRunEstimate? Find(string code);
    void Save(PayRunEstimate item);
    bool Remove(string code);
    List<PayRunEstimate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunEstimateStore(Dictionary<string, PayRunEstimate> items, List<string> order) : IPayRunEstimateStore
{
    public Dictionary<string, PayRunEstimate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunEstimate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunEstimate item)
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

    public List<PayRunEstimate> List()
    {
        var outValue = new List<PayRunEstimate>();
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
public sealed class CachedPayRunEstimateStore(MemoryPayRunEstimateStore inner, PayRunEstimateCache cache, int hits, int misses) : IPayRunEstimateStore
{
    public MemoryPayRunEstimateStore Inner { get; set; } = inner;
    public PayRunEstimateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunEstimate? Find(string code)
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

    public void Save(PayRunEstimate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunEstimate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run estimates. Empty fields match all.</summary>
public sealed record PayRunEstimateQuery(string Team, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunEstimate item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunEstimate> Apply(List<PayRunEstimate> items)
    {
        var matched = Collections.Filter(items, (PayRunEstimate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run estimate store.</summary>
public static class PayRunEstimateStore
{
    public static IPayRunEstimateStore NewMemoryPayRunEstimateStore()
    {
        return new MemoryPayRunEstimateStore(new Dictionary<string, PayRunEstimate>(), new List<string>());
    }

    public static IPayRunEstimateStore NewCachedPayRunEstimateStore(MemoryPayRunEstimateStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunEstimateStore(inner, PayRunEstimateCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayRunEstimate> FindPayRunEstimates(IPayRunEstimateStore store, PayRunEstimateQuery query)
    {
        return query.Apply(store.List());
    }
}
