namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run baselines by code.</summary>
public interface IPayRunBaselineStore
{
    PayRunBaseline? Find(string code);
    void Save(PayRunBaseline item);
    bool Remove(string code);
    List<PayRunBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunBaselineStore(Dictionary<string, PayRunBaseline> items, List<string> order) : IPayRunBaselineStore
{
    public Dictionary<string, PayRunBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunBaseline item)
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

    public List<PayRunBaseline> List()
    {
        var outValue = new List<PayRunBaseline>();
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
public sealed class CachedPayRunBaselineStore(MemoryPayRunBaselineStore inner, PayRunBaselineCache cache, int hits, int misses) : IPayRunBaselineStore
{
    public MemoryPayRunBaselineStore Inner { get; set; } = inner;
    public PayRunBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunBaseline? Find(string code)
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

    public void Save(PayRunBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run baselines. Empty fields match all.</summary>
public sealed record PayRunBaselineQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunBaseline> Apply(List<PayRunBaseline> items)
    {
        var matched = Collections.Filter(items, (PayRunBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run baseline store.</summary>
public static class PayRunBaselineStore
{
    public static IPayRunBaselineStore NewMemoryPayRunBaselineStore()
    {
        return new MemoryPayRunBaselineStore(new Dictionary<string, PayRunBaseline>(), new List<string>());
    }

    public static IPayRunBaselineStore NewCachedPayRunBaselineStore(MemoryPayRunBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunBaselineStore(inner, PayRunBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayRunBaseline> FindPayRunBaselines(IPayRunBaselineStore store, PayRunBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
