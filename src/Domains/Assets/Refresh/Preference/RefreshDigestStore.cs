namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh digests by code.</summary>
public interface IRefreshDigestStore
{
    RefreshDigest? Find(string code);
    void Save(RefreshDigest item);
    bool Remove(string code);
    List<RefreshDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshDigestStore(Dictionary<string, RefreshDigest> items, List<string> order) : IRefreshDigestStore
{
    public Dictionary<string, RefreshDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshDigest item)
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

    public List<RefreshDigest> List()
    {
        var outValue = new List<RefreshDigest>();
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
public sealed class CachedRefreshDigestStore(MemoryRefreshDigestStore inner, RefreshDigestCache cache, int hits, int misses) : IRefreshDigestStore
{
    public MemoryRefreshDigestStore Inner { get; set; } = inner;
    public RefreshDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshDigest? Find(string code)
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

    public void Save(RefreshDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh digests. Empty fields match all.</summary>
public sealed record RefreshDigestQuery(string Title, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshDigest> Apply(List<RefreshDigest> items)
    {
        var matched = Collections.Filter(items, (RefreshDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh digest store.</summary>
public static class RefreshDigestStore
{
    public static IRefreshDigestStore NewMemoryRefreshDigestStore()
    {
        return new MemoryRefreshDigestStore(new Dictionary<string, RefreshDigest>(), new List<string>());
    }

    public static IRefreshDigestStore NewCachedRefreshDigestStore(MemoryRefreshDigestStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshDigestStore(inner, RefreshDigestCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RefreshDigest> FindRefreshDigests(IRefreshDigestStore store, RefreshDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
