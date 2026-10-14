namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient pay run requests by code.</summary>
public interface ILenientPayRunRequestStore
{
    LenientPayRunRequest? Find(string code);
    void Save(LenientPayRunRequest item);
    bool Remove(string code);
    List<LenientPayRunRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientPayRunRequestStore(Dictionary<string, LenientPayRunRequest> items, List<string> order) : ILenientPayRunRequestStore
{
    public Dictionary<string, LenientPayRunRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientPayRunRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientPayRunRequest item)
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

    public List<LenientPayRunRequest> List()
    {
        var outValue = new List<LenientPayRunRequest>();
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
public sealed class CachedLenientPayRunRequestStore(MemoryLenientPayRunRequestStore inner, LenientPayRunRequestCache cache, int hits, int misses) : ILenientPayRunRequestStore
{
    public MemoryLenientPayRunRequestStore Inner { get; set; } = inner;
    public LenientPayRunRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientPayRunRequest? Find(string code)
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

    public void Save(LenientPayRunRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientPayRunRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient pay run requests. Empty fields match all.</summary>
public sealed record LenientPayRunRequestQuery(string Channel, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(LenientPayRunRequest item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<LenientPayRunRequest> Apply(List<LenientPayRunRequest> items)
    {
        var matched = Collections.Filter(items, (LenientPayRunRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient pay run request store.</summary>
public static class LenientPayRunRequestStore
{
    public static ILenientPayRunRequestStore NewMemoryLenientPayRunRequestStore()
    {
        return new MemoryLenientPayRunRequestStore(
            new Dictionary<string, LenientPayRunRequest>(),
            new List<string>());
    }

    public static ILenientPayRunRequestStore NewCachedLenientPayRunRequestStore(MemoryLenientPayRunRequestStore inner, int ttl, IClock clock)
    {
        return new CachedLenientPayRunRequestStore(inner, LenientPayRunRequestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LenientPayRunRequest> FindLenientPayRunRequests(ILenientPayRunRequestStore store, LenientPayRunRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
