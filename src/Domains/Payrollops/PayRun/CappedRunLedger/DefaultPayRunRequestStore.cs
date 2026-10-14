namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default pay run requests by code.</summary>
public interface IDefaultPayRunRequestStore
{
    DefaultPayRunRequest? Find(string code);
    void Save(DefaultPayRunRequest item);
    bool Remove(string code);
    List<DefaultPayRunRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPayRunRequestStore(Dictionary<string, DefaultPayRunRequest> items, List<string> order) : IDefaultPayRunRequestStore
{
    public Dictionary<string, DefaultPayRunRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPayRunRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPayRunRequest item)
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

    public List<DefaultPayRunRequest> List()
    {
        var outValue = new List<DefaultPayRunRequest>();
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
public sealed class CachedDefaultPayRunRequestStore(MemoryDefaultPayRunRequestStore inner, DefaultPayRunRequestCache cache, int hits, int misses) : IDefaultPayRunRequestStore
{
    public MemoryDefaultPayRunRequestStore Inner { get; set; } = inner;
    public DefaultPayRunRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPayRunRequest? Find(string code)
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

    public void Save(DefaultPayRunRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPayRunRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default pay run requests. Empty fields match all.</summary>
public sealed record DefaultPayRunRequestQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPayRunRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPayRunRequest> Apply(List<DefaultPayRunRequest> items)
    {
        var matched = Collections.Filter(items, (DefaultPayRunRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default pay run request store.</summary>
public static class DefaultPayRunRequestStore
{
    public static IDefaultPayRunRequestStore NewMemoryDefaultPayRunRequestStore()
    {
        return new MemoryDefaultPayRunRequestStore(
            new Dictionary<string, DefaultPayRunRequest>(),
            new List<string>());
    }

    public static IDefaultPayRunRequestStore NewCachedDefaultPayRunRequestStore(MemoryDefaultPayRunRequestStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPayRunRequestStore(inner, DefaultPayRunRequestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<DefaultPayRunRequest> FindDefaultPayRunRequests(IDefaultPayRunRequestStore store, DefaultPayRunRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
