namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant requests by code.</summary>
public interface IEquityGrantRequestStore
{
    EquityGrantRequest? Find(string code);
    void Save(EquityGrantRequest item);
    bool Remove(string code);
    List<EquityGrantRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantRequestStore(Dictionary<string, EquityGrantRequest> items, List<string> order) : IEquityGrantRequestStore
{
    public Dictionary<string, EquityGrantRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantRequest item)
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

    public List<EquityGrantRequest> List()
    {
        var outValue = new List<EquityGrantRequest>();
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
public sealed class CachedEquityGrantRequestStore(MemoryEquityGrantRequestStore inner, EquityGrantRequestCache cache, int hits, int misses) : IEquityGrantRequestStore
{
    public MemoryEquityGrantRequestStore Inner { get; set; } = inner;
    public EquityGrantRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantRequest? Find(string code)
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

    public void Save(EquityGrantRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant requests. Empty fields match all.</summary>
public sealed record EquityGrantRequestQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantRequest> Apply(List<EquityGrantRequest> items)
    {
        var matched = Collections.Filter(items, (EquityGrantRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant request store.</summary>
public static class EquityGrantRequestStore
{
    public static IEquityGrantRequestStore NewMemoryEquityGrantRequestStore()
    {
        return new MemoryEquityGrantRequestStore(new Dictionary<string, EquityGrantRequest>(), new List<string>());
    }

    public static IEquityGrantRequestStore NewCachedEquityGrantRequestStore(MemoryEquityGrantRequestStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantRequestStore(inner, EquityGrantRequestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<EquityGrantRequest> FindEquityGrantRequests(IEquityGrantRequestStore store, EquityGrantRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
