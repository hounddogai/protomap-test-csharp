namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band rules by code.</summary>
public interface IPayBandRuleStore
{
    PayBandRule? Find(string code);
    void Save(PayBandRule item);
    bool Remove(string code);
    List<PayBandRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandRuleStore(Dictionary<string, PayBandRule> items, List<string> order) : IPayBandRuleStore
{
    public Dictionary<string, PayBandRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandRule item)
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

    public List<PayBandRule> List()
    {
        var outValue = new List<PayBandRule>();
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
public sealed class CachedPayBandRuleStore(MemoryPayBandRuleStore inner, PayBandRuleCache cache, int hits, int misses) : IPayBandRuleStore
{
    public MemoryPayBandRuleStore Inner { get; set; } = inner;
    public PayBandRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandRule? Find(string code)
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

    public void Save(PayBandRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band rules. Empty fields match all.</summary>
public sealed record PayBandRuleQuery(string Source, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandRule item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandRule> Apply(List<PayBandRule> items)
    {
        var matched = Collections.Filter(items, (PayBandRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band rule store.</summary>
public static class PayBandRuleStore
{
    public static IPayBandRuleStore NewMemoryPayBandRuleStore()
    {
        return new MemoryPayBandRuleStore(new Dictionary<string, PayBandRule>(), new List<string>());
    }

    public static IPayBandRuleStore NewCachedPayBandRuleStore(MemoryPayBandRuleStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandRuleStore(inner, PayBandRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PayBandRule> FindPayBandRules(IPayBandRuleStore store, PayBandRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
