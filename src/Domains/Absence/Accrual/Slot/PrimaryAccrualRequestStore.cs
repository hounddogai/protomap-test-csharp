namespace Acme.Workforce.Absence.Accrual.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary accrual requests by code.</summary>
public interface IPrimaryAccrualRequestStore
{
    PrimaryAccrualRequest? Find(string code);
    void Save(PrimaryAccrualRequest item);
    bool Remove(string code);
    List<PrimaryAccrualRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAccrualRequestStore(Dictionary<string, PrimaryAccrualRequest> items, List<string> order) : IPrimaryAccrualRequestStore
{
    public Dictionary<string, PrimaryAccrualRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAccrualRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAccrualRequest item)
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

    public List<PrimaryAccrualRequest> List()
    {
        var outValue = new List<PrimaryAccrualRequest>();
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
public sealed class CachedPrimaryAccrualRequestStore(MemoryPrimaryAccrualRequestStore inner, PrimaryAccrualRequestCache cache, int hits, int misses) : IPrimaryAccrualRequestStore
{
    public MemoryPrimaryAccrualRequestStore Inner { get; set; } = inner;
    public PrimaryAccrualRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAccrualRequest? Find(string code)
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

    public void Save(PrimaryAccrualRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAccrualRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary accrual requests. Empty fields match all.</summary>
public sealed record PrimaryAccrualRequestQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAccrualRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAccrualRequest> Apply(List<PrimaryAccrualRequest> items)
    {
        var matched = Collections.Filter(items, (PrimaryAccrualRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary accrual request store.</summary>
public static class PrimaryAccrualRequestStore
{
    public static IPrimaryAccrualRequestStore NewMemoryPrimaryAccrualRequestStore()
    {
        return new MemoryPrimaryAccrualRequestStore(
            new Dictionary<string, PrimaryAccrualRequest>(),
            new List<string>());
    }

    public static IPrimaryAccrualRequestStore NewCachedPrimaryAccrualRequestStore(MemoryPrimaryAccrualRequestStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAccrualRequestStore(inner, PrimaryAccrualRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PrimaryAccrualRequest> FindPrimaryAccrualRequests(IPrimaryAccrualRequestStore store, PrimaryAccrualRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
