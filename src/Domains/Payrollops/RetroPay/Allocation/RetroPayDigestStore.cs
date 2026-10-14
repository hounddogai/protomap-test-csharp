namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay digests by code.</summary>
public interface IRetroPayDigestStore
{
    RetroPayDigest? Find(string code);
    void Save(RetroPayDigest item);
    bool Remove(string code);
    List<RetroPayDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayDigestStore(Dictionary<string, RetroPayDigest> items, List<string> order) : IRetroPayDigestStore
{
    public Dictionary<string, RetroPayDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayDigest item)
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

    public List<RetroPayDigest> List()
    {
        var outValue = new List<RetroPayDigest>();
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
public sealed class CachedRetroPayDigestStore(MemoryRetroPayDigestStore inner, RetroPayDigestCache cache, int hits, int misses) : IRetroPayDigestStore
{
    public MemoryRetroPayDigestStore Inner { get; set; } = inner;
    public RetroPayDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayDigest? Find(string code)
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

    public void Save(RetroPayDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay digests. Empty fields match all.</summary>
public sealed record RetroPayDigestQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayDigest item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RetroPayDigest> Apply(List<RetroPayDigest> items)
    {
        var matched = Collections.Filter(items, (RetroPayDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay digest store.</summary>
public static class RetroPayDigestStore
{
    public static IRetroPayDigestStore NewMemoryRetroPayDigestStore()
    {
        return new MemoryRetroPayDigestStore(new Dictionary<string, RetroPayDigest>(), new List<string>());
    }

    public static IRetroPayDigestStore NewCachedRetroPayDigestStore(MemoryRetroPayDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayDigestStore(inner, RetroPayDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RetroPayDigest> FindRetroPayDigests(IRetroPayDigestStore store, RetroPayDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
