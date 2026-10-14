namespace Acme.Workforce.Absence.Balance.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance requests by code.</summary>
public interface IBalanceRequestStore
{
    BalanceRequest? Find(string code);
    void Save(BalanceRequest item);
    bool Remove(string code);
    List<BalanceRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceRequestStore(Dictionary<string, BalanceRequest> items, List<string> order) : IBalanceRequestStore
{
    public Dictionary<string, BalanceRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceRequest item)
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

    public List<BalanceRequest> List()
    {
        var outValue = new List<BalanceRequest>();
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
public sealed class CachedBalanceRequestStore(MemoryBalanceRequestStore inner, BalanceRequestCache cache, int hits, int misses) : IBalanceRequestStore
{
    public MemoryBalanceRequestStore Inner { get; set; } = inner;
    public BalanceRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceRequest? Find(string code)
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

    public void Save(BalanceRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance requests. Empty fields match all.</summary>
public sealed record BalanceRequestQuery(string Segment, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceRequest item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BalanceRequest> Apply(List<BalanceRequest> items)
    {
        var matched = Collections.Filter(items, (BalanceRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance request store.</summary>
public static class BalanceRequestStore
{
    public static IBalanceRequestStore NewMemoryBalanceRequestStore()
    {
        return new MemoryBalanceRequestStore(new Dictionary<string, BalanceRequest>(), new List<string>());
    }

    public static IBalanceRequestStore NewCachedBalanceRequestStore(MemoryBalanceRequestStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceRequestStore(inner, BalanceRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BalanceRequest> FindBalanceRequests(IBalanceRequestStore store, BalanceRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
