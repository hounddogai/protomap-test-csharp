namespace Acme.Workforce.Payrollops.RetroPay.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps retro pay windows by code.</summary>
public interface IRetroPayWindowStore
{
    RetroPayWindow? Find(string code);
    void Save(RetroPayWindow item);
    bool Remove(string code);
    List<RetroPayWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRetroPayWindowStore(Dictionary<string, RetroPayWindow> items, List<string> order) : IRetroPayWindowStore
{
    public Dictionary<string, RetroPayWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RetroPayWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RetroPayWindow item)
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

    public List<RetroPayWindow> List()
    {
        var outValue = new List<RetroPayWindow>();
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
public sealed class CachedRetroPayWindowStore(MemoryRetroPayWindowStore inner, RetroPayWindowCache cache, int hits, int misses) : IRetroPayWindowStore
{
    public MemoryRetroPayWindowStore Inner { get; set; } = inner;
    public RetroPayWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RetroPayWindow? Find(string code)
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

    public void Save(RetroPayWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RetroPayWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects retro pay windows. Empty fields match all.</summary>
public sealed record RetroPayWindowQuery(string Source, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(RetroPayWindow item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<RetroPayWindow> Apply(List<RetroPayWindow> items)
    {
        var matched = Collections.Filter(items, (RetroPayWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of retro pay window store.</summary>
public static class RetroPayWindowStore
{
    public static IRetroPayWindowStore NewMemoryRetroPayWindowStore()
    {
        return new MemoryRetroPayWindowStore(new Dictionary<string, RetroPayWindow>(), new List<string>());
    }

    public static IRetroPayWindowStore NewCachedRetroPayWindowStore(MemoryRetroPayWindowStore inner, int ttl, IClock clock)
    {
        return new CachedRetroPayWindowStore(inner, RetroPayWindowCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RetroPayWindow> FindRetroPayWindows(IRetroPayWindowStore store, RetroPayWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
