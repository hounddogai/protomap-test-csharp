namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh profiles by code.</summary>
public interface IRefreshProfileStore
{
    RefreshProfile? Find(string code);
    void Save(RefreshProfile item);
    bool Remove(string code);
    List<RefreshProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshProfileStore(Dictionary<string, RefreshProfile> items, List<string> order) : IRefreshProfileStore
{
    public Dictionary<string, RefreshProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshProfile item)
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

    public List<RefreshProfile> List()
    {
        var outValue = new List<RefreshProfile>();
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
public sealed class CachedRefreshProfileStore(MemoryRefreshProfileStore inner, RefreshProfileCache cache, int hits, int misses) : IRefreshProfileStore
{
    public MemoryRefreshProfileStore Inner { get; set; } = inner;
    public RefreshProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshProfile? Find(string code)
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

    public void Save(RefreshProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh profiles. Empty fields match all.</summary>
public sealed record RefreshProfileQuery(string Title, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<RefreshProfile> Apply(List<RefreshProfile> items)
    {
        var matched = Collections.Filter(items, (RefreshProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh profile store.</summary>
public static class RefreshProfileStore
{
    public static IRefreshProfileStore NewMemoryRefreshProfileStore()
    {
        return new MemoryRefreshProfileStore(new Dictionary<string, RefreshProfile>(), new List<string>());
    }

    public static IRefreshProfileStore NewCachedRefreshProfileStore(MemoryRefreshProfileStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshProfileStore(inner, RefreshProfileCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshProfile> FindRefreshProfiles(IRefreshProfileStore store, RefreshProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
