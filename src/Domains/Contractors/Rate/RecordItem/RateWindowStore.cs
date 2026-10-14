namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate windows by code.</summary>
public interface IRateWindowStore
{
    RateWindow? Find(string code);
    void Save(RateWindow item);
    bool Remove(string code);
    List<RateWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateWindowStore(Dictionary<string, RateWindow> items, List<string> order) : IRateWindowStore
{
    public Dictionary<string, RateWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateWindow item)
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

    public List<RateWindow> List()
    {
        var outValue = new List<RateWindow>();
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
public sealed class CachedRateWindowStore(MemoryRateWindowStore inner, RateWindowCache cache, int hits, int misses) : IRateWindowStore
{
    public MemoryRateWindowStore Inner { get; set; } = inner;
    public RateWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateWindow? Find(string code)
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

    public void Save(RateWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate windows. Empty fields match all.</summary>
public sealed record RateWindowQuery(string Title, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(RateWindow item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RateWindow> Apply(List<RateWindow> items)
    {
        var matched = Collections.Filter(items, (RateWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate window store.</summary>
public static class RateWindowStore
{
    public static IRateWindowStore NewMemoryRateWindowStore()
    {
        return new MemoryRateWindowStore(new Dictionary<string, RateWindow>(), new List<string>());
    }

    public static IRateWindowStore NewCachedRateWindowStore(MemoryRateWindowStore inner, int ttl, IClock clock)
    {
        return new CachedRateWindowStore(inner, RateWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<RateWindow> FindRateWindows(IRateWindowStore store, RateWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
