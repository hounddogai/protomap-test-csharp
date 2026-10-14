namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh requests by code.</summary>
public interface IRefreshRequestStore
{
    RefreshRequest? Find(string code);
    void Save(RefreshRequest item);
    bool Remove(string code);
    List<RefreshRequest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshRequestStore(Dictionary<string, RefreshRequest> items, List<string> order) : IRefreshRequestStore
{
    public Dictionary<string, RefreshRequest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshRequest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshRequest item)
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

    public List<RefreshRequest> List()
    {
        var outValue = new List<RefreshRequest>();
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
public sealed class CachedRefreshRequestStore(MemoryRefreshRequestStore inner, RefreshRequestCache cache, int hits, int misses) : IRefreshRequestStore
{
    public MemoryRefreshRequestStore Inner { get; set; } = inner;
    public RefreshRequestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshRequest? Find(string code)
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

    public void Save(RefreshRequest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshRequest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh requests. Empty fields match all.</summary>
public sealed record RefreshRequestQuery(string Title, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshRequest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RefreshRequest> Apply(List<RefreshRequest> items)
    {
        var matched = Collections.Filter(items, (RefreshRequest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh request store.</summary>
public static class RefreshRequestStore
{
    public static IRefreshRequestStore NewMemoryRefreshRequestStore()
    {
        return new MemoryRefreshRequestStore(new Dictionary<string, RefreshRequest>(), new List<string>());
    }

    public static IRefreshRequestStore NewCachedRefreshRequestStore(MemoryRefreshRequestStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshRequestStore(inner, RefreshRequestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RefreshRequest> FindRefreshRequests(IRefreshRequestStore store, RefreshRequestQuery query)
    {
        return query.Apply(store.List());
    }
}
