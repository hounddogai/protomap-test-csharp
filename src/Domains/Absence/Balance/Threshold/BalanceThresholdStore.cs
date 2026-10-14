namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance thresholds by code.</summary>
public interface IBalanceThresholdStore
{
    BalanceThreshold? Find(string code);
    void Save(BalanceThreshold item);
    bool Remove(string code);
    List<BalanceThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceThresholdStore(Dictionary<string, BalanceThreshold> items, List<string> order) : IBalanceThresholdStore
{
    public Dictionary<string, BalanceThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceThreshold item)
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

    public List<BalanceThreshold> List()
    {
        var outValue = new List<BalanceThreshold>();
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
public sealed class CachedBalanceThresholdStore(MemoryBalanceThresholdStore inner, BalanceThresholdCache cache, int hits, int misses) : IBalanceThresholdStore
{
    public MemoryBalanceThresholdStore Inner { get; set; } = inner;
    public BalanceThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceThreshold? Find(string code)
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

    public void Save(BalanceThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance thresholds. Empty fields match all.</summary>
public sealed record BalanceThresholdQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceThreshold item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceThreshold> Apply(List<BalanceThreshold> items)
    {
        var matched = Collections.Filter(items, (BalanceThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance threshold store.</summary>
public static class BalanceThresholdStore
{
    public static IBalanceThresholdStore NewMemoryBalanceThresholdStore()
    {
        return new MemoryBalanceThresholdStore(new Dictionary<string, BalanceThreshold>(), new List<string>());
    }

    public static IBalanceThresholdStore NewCachedBalanceThresholdStore(MemoryBalanceThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceThresholdStore(inner, BalanceThresholdCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BalanceThreshold> FindBalanceThresholds(IBalanceThresholdStore store, BalanceThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
