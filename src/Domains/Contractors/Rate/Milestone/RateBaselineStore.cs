namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate baselines by code.</summary>
public interface IRateBaselineStore
{
    RateBaseline? Find(string code);
    void Save(RateBaseline item);
    bool Remove(string code);
    List<RateBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateBaselineStore(Dictionary<string, RateBaseline> items, List<string> order) : IRateBaselineStore
{
    public Dictionary<string, RateBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateBaseline item)
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

    public List<RateBaseline> List()
    {
        var outValue = new List<RateBaseline>();
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
public sealed class CachedRateBaselineStore(MemoryRateBaselineStore inner, RateBaselineCache cache, int hits, int misses) : IRateBaselineStore
{
    public MemoryRateBaselineStore Inner { get; set; } = inner;
    public RateBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateBaseline? Find(string code)
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

    public void Save(RateBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate baselines. Empty fields match all.</summary>
public sealed record RateBaselineQuery(string Locale, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RateBaseline item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<RateBaseline> Apply(List<RateBaseline> items)
    {
        var matched = Collections.Filter(items, (RateBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate baseline store.</summary>
public static class RateBaselineStore
{
    public static IRateBaselineStore NewMemoryRateBaselineStore()
    {
        return new MemoryRateBaselineStore(new Dictionary<string, RateBaseline>(), new List<string>());
    }

    public static IRateBaselineStore NewCachedRateBaselineStore(MemoryRateBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedRateBaselineStore(inner, RateBaselineCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RateBaseline> FindRateBaselines(IRateBaselineStore store, RateBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
