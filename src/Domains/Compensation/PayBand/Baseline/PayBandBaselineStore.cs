namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band baselines by code.</summary>
public interface IPayBandBaselineStore
{
    PayBandBaseline? Find(string code);
    void Save(PayBandBaseline item);
    bool Remove(string code);
    List<PayBandBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandBaselineStore(Dictionary<string, PayBandBaseline> items, List<string> order) : IPayBandBaselineStore
{
    public Dictionary<string, PayBandBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandBaseline item)
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

    public List<PayBandBaseline> List()
    {
        var outValue = new List<PayBandBaseline>();
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
public sealed class CachedPayBandBaselineStore(MemoryPayBandBaselineStore inner, PayBandBaselineCache cache, int hits, int misses) : IPayBandBaselineStore
{
    public MemoryPayBandBaselineStore Inner { get; set; } = inner;
    public PayBandBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandBaseline? Find(string code)
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

    public void Save(PayBandBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band baselines. Empty fields match all.</summary>
public sealed record PayBandBaselineQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandBaseline> Apply(List<PayBandBaseline> items)
    {
        var matched = Collections.Filter(items, (PayBandBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band baseline store.</summary>
public static class PayBandBaselineStore
{
    public static IPayBandBaselineStore NewMemoryPayBandBaselineStore()
    {
        return new MemoryPayBandBaselineStore(new Dictionary<string, PayBandBaseline>(), new List<string>());
    }

    public static IPayBandBaselineStore NewCachedPayBandBaselineStore(MemoryPayBandBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandBaselineStore(inner, PayBandBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayBandBaseline> FindPayBandBaselines(IPayBandBaselineStore store, PayBandBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
