namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps accrual requests by code.</summary>
public interface IAccrualRequestStore
{
    AccrualRequest? Find(string code);
    void Save(AccrualRequest item);
    bool Remove(string code);
    List<AccrualRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccrualRequestStore(Dictionary<string, AccrualRequest> items, List<string> order) : IAccrualRequestStore
{
    public Dictionary<string, AccrualRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccrualRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccrualRequest item)
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

    public List<AccrualRequest> List()
    {
        var outValue = new List<AccrualRequest>();
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
public sealed class CachedAccrualRequestStore(MemoryAccrualRequestStore inner, AccrualRequestCache cache, int hits, int misses) : IAccrualRequestStore
{
    public MemoryAccrualRequestStore Inner { get; set; } = inner;
    public AccrualRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccrualRequest? Find(string code)
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

    public void Save(AccrualRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccrualRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects accrual requests. Empty fields match all.</summary>
public sealed record AccrualRequestQuery(string Name, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(AccrualRequest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccrualRequest> Apply(List<AccrualRequest> items)
    {
        var matched = Collections.Filter(items, (AccrualRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of accrual request store.</summary>
public static class AccrualRequestStore
{
    public static IAccrualRequestStore NewMemoryAccrualRequestStore()
    {
        return new MemoryAccrualRequestStore(new Dictionary<string, AccrualRequest>(), new List<string>());
    }

    public static IAccrualRequestStore NewCachedAccrualRequestStore(MemoryAccrualRequestStore inner, int ttl, IClock clock)
    {
        return new CachedAccrualRequestStore(inner, AccrualRequestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AccrualRequest> FindAccrualRequests(IAccrualRequestStore store, AccrualRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
