namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default bonus pool requests by code.</summary>
public interface IDefaultBonusPoolRequestStore
{
    DefaultBonusPoolRequest? Find(string code);
    void Save(DefaultBonusPoolRequest item);
    bool Remove(string code);
    List<DefaultBonusPoolRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultBonusPoolRequestStore(Dictionary<string, DefaultBonusPoolRequest> items, List<string> order) : IDefaultBonusPoolRequestStore
{
    public Dictionary<string, DefaultBonusPoolRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultBonusPoolRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultBonusPoolRequest item)
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

    public List<DefaultBonusPoolRequest> List()
    {
        var outValue = new List<DefaultBonusPoolRequest>();
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
public sealed class CachedDefaultBonusPoolRequestStore(MemoryDefaultBonusPoolRequestStore inner, DefaultBonusPoolRequestCache cache, int hits, int misses) : IDefaultBonusPoolRequestStore
{
    public MemoryDefaultBonusPoolRequestStore Inner { get; set; } = inner;
    public DefaultBonusPoolRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultBonusPoolRequest? Find(string code)
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

    public void Save(DefaultBonusPoolRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultBonusPoolRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default bonus pool requests. Empty fields match all.</summary>
public sealed record DefaultBonusPoolRequestQuery(string Name, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultBonusPoolRequest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultBonusPoolRequest> Apply(List<DefaultBonusPoolRequest> items)
    {
        var matched = Collections.Filter(items, (DefaultBonusPoolRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default bonus pool request store.</summary>
public static class DefaultBonusPoolRequestStore
{
    public static IDefaultBonusPoolRequestStore NewMemoryDefaultBonusPoolRequestStore()
    {
        return new MemoryDefaultBonusPoolRequestStore(
            new Dictionary<string, DefaultBonusPoolRequest>(),
            new List<string>());
    }

    public static IDefaultBonusPoolRequestStore NewCachedDefaultBonusPoolRequestStore(MemoryDefaultBonusPoolRequestStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultBonusPoolRequestStore(
            inner,
            DefaultBonusPoolRequestCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultBonusPoolRequest> FindDefaultBonusPoolRequests(IDefaultBonusPoolRequestStore store, DefaultBonusPoolRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
