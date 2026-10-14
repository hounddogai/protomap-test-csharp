namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band scores by code.</summary>
public interface IPayBandScoreStore
{
    PayBandScore? Find(string code);
    void Save(PayBandScore item);
    bool Remove(string code);
    List<PayBandScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandScoreStore(Dictionary<string, PayBandScore> items, List<string> order) : IPayBandScoreStore
{
    public Dictionary<string, PayBandScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandScore item)
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

    public List<PayBandScore> List()
    {
        var outValue = new List<PayBandScore>();
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
public sealed class CachedPayBandScoreStore(MemoryPayBandScoreStore inner, PayBandScoreCache cache, int hits, int misses) : IPayBandScoreStore
{
    public MemoryPayBandScoreStore Inner { get; set; } = inner;
    public PayBandScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandScore? Find(string code)
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

    public void Save(PayBandScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band scores. Empty fields match all.</summary>
public sealed record PayBandScoreQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandScore item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandScore> Apply(List<PayBandScore> items)
    {
        var matched = Collections.Filter(items, (PayBandScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band score store.</summary>
public static class PayBandScoreStore
{
    public static IPayBandScoreStore NewMemoryPayBandScoreStore()
    {
        return new MemoryPayBandScoreStore(new Dictionary<string, PayBandScore>(), new List<string>());
    }

    public static IPayBandScoreStore NewCachedPayBandScoreStore(MemoryPayBandScoreStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandScoreStore(inner, PayBandScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayBandScore> FindPayBandScores(IPayBandScoreStore store, PayBandScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
