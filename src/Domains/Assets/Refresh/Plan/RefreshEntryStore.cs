namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps refresh entries by code.</summary>
public interface IRefreshEntryStore
{
    RefreshEntry? Find(string code);
    void Save(RefreshEntry item);
    bool Remove(string code);
    List<RefreshEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRefreshEntryStore(Dictionary<string, RefreshEntry> items, List<string> order) : IRefreshEntryStore
{
    public Dictionary<string, RefreshEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RefreshEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RefreshEntry item)
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

    public List<RefreshEntry> List()
    {
        var outValue = new List<RefreshEntry>();
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
public sealed class CachedRefreshEntryStore(MemoryRefreshEntryStore inner, RefreshEntryCache cache, int hits, int misses) : IRefreshEntryStore
{
    public MemoryRefreshEntryStore Inner { get; set; } = inner;
    public RefreshEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RefreshEntry? Find(string code)
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

    public void Save(RefreshEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RefreshEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects refresh entries. Empty fields match all.</summary>
public sealed record RefreshEntryQuery(string Region, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(RefreshEntry item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RefreshEntry> Apply(List<RefreshEntry> items)
    {
        var matched = Collections.Filter(items, (RefreshEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of refresh entry store.</summary>
public static class RefreshEntryStore
{
    public static IRefreshEntryStore NewMemoryRefreshEntryStore()
    {
        return new MemoryRefreshEntryStore(new Dictionary<string, RefreshEntry>(), new List<string>());
    }

    public static IRefreshEntryStore NewCachedRefreshEntryStore(MemoryRefreshEntryStore inner, int ttl, IClock clock)
    {
        return new CachedRefreshEntryStore(inner, RefreshEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RefreshEntry> FindRefreshEntries(IRefreshEntryStore store, RefreshEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
