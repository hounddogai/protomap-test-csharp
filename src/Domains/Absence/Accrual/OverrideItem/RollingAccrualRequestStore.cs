namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling accrual requests by code.</summary>
public interface IRollingAccrualRequestStore
{
    RollingAccrualRequest? Find(string code);
    void Save(RollingAccrualRequest item);
    bool Remove(string code);
    List<RollingAccrualRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAccrualRequestStore(Dictionary<string, RollingAccrualRequest> items, List<string> order) : IRollingAccrualRequestStore
{
    public Dictionary<string, RollingAccrualRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAccrualRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAccrualRequest item)
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

    public List<RollingAccrualRequest> List()
    {
        var outValue = new List<RollingAccrualRequest>();
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
public sealed class CachedRollingAccrualRequestStore(MemoryRollingAccrualRequestStore inner, RollingAccrualRequestCache cache, int hits, int misses) : IRollingAccrualRequestStore
{
    public MemoryRollingAccrualRequestStore Inner { get; set; } = inner;
    public RollingAccrualRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAccrualRequest? Find(string code)
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

    public void Save(RollingAccrualRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAccrualRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling accrual requests. Empty fields match all.</summary>
public sealed record RollingAccrualRequestQuery(string Country, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAccrualRequest item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAccrualRequest> Apply(List<RollingAccrualRequest> items)
    {
        var matched = Collections.Filter(items, (RollingAccrualRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling accrual request store.</summary>
public static class RollingAccrualRequestStore
{
    public static IRollingAccrualRequestStore NewMemoryRollingAccrualRequestStore()
    {
        return new MemoryRollingAccrualRequestStore(
            new Dictionary<string, RollingAccrualRequest>(),
            new List<string>());
    }

    public static IRollingAccrualRequestStore NewCachedRollingAccrualRequestStore(MemoryRollingAccrualRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAccrualRequestStore(inner, RollingAccrualRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RollingAccrualRequest> FindRollingAccrualRequests(IRollingAccrualRequestStore store, RollingAccrualRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
