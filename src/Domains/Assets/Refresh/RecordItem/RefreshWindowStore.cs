namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh windows by code.</summary>
public interface IRefreshWindowStore
{
    RefreshWindow? Find(string code);
    void Save(RefreshWindow item);
    bool Remove(string code);
    List<RefreshWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshWindowStore(Dictionary<string, RefreshWindow> items, List<string> order) : IRefreshWindowStore
{
    public Dictionary<string, RefreshWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshWindow item)
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

    public List<RefreshWindow> List()
    {
        var outValue = new List<RefreshWindow>();
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
public sealed class CachedRefreshWindowStore(MemoryRefreshWindowStore inner, RefreshWindowCache cache, int hits, int misses) : IRefreshWindowStore
{
    public MemoryRefreshWindowStore Inner { get; set; } = inner;
    public RefreshWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshWindow? Find(string code)
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

    public void Save(RefreshWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh windows. Empty fields match all.</summary>
public sealed record RefreshWindowQuery(string Locale, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshWindow item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshWindow> Apply(List<RefreshWindow> items)
    {
        var matched = Collections.Filter(items, (RefreshWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh window store.</summary>
public static class RefreshWindowStore
{
    public static IRefreshWindowStore NewMemoryRefreshWindowStore()
    {
        return new MemoryRefreshWindowStore(new Dictionary<string, RefreshWindow>(), new List<string>());
    }

    public static IRefreshWindowStore NewCachedRefreshWindowStore(MemoryRefreshWindowStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshWindowStore(inner, RefreshWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<RefreshWindow> FindRefreshWindows(IRefreshWindowStore store, RefreshWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
