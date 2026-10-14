namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance baselines by code.</summary>
public interface IBalanceBaselineStore
{
    BalanceBaseline? Find(string code);
    void Save(BalanceBaseline item);
    bool Remove(string code);
    List<BalanceBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceBaselineStore(Dictionary<string, BalanceBaseline> items, List<string> order) : IBalanceBaselineStore
{
    public Dictionary<string, BalanceBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceBaseline item)
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

    public List<BalanceBaseline> List()
    {
        var outValue = new List<BalanceBaseline>();
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
public sealed class CachedBalanceBaselineStore(MemoryBalanceBaselineStore inner, BalanceBaselineCache cache, int hits, int misses) : IBalanceBaselineStore
{
    public MemoryBalanceBaselineStore Inner { get; set; } = inner;
    public BalanceBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceBaseline? Find(string code)
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

    public void Save(BalanceBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance baselines. Empty fields match all.</summary>
public sealed record BalanceBaselineQuery(string Region, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceBaseline item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceBaseline> Apply(List<BalanceBaseline> items)
    {
        var matched = Collections.Filter(items, (BalanceBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance baseline store.</summary>
public static class BalanceBaselineStore
{
    public static IBalanceBaselineStore NewMemoryBalanceBaselineStore()
    {
        return new MemoryBalanceBaselineStore(new Dictionary<string, BalanceBaseline>(), new List<string>());
    }

    public static IBalanceBaselineStore NewCachedBalanceBaselineStore(MemoryBalanceBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceBaselineStore(inner, BalanceBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<BalanceBaseline> FindBalanceBaselines(IBalanceBaselineStore store, BalanceBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
