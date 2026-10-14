namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount windows by code.</summary>
public interface IHeadcountWindowStore
{
    HeadcountWindow? Find(string code);
    void Save(HeadcountWindow item);
    bool Remove(string code);
    List<HeadcountWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountWindowStore(Dictionary<string, HeadcountWindow> items, List<string> order) : IHeadcountWindowStore
{
    public Dictionary<string, HeadcountWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountWindow item)
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

    public List<HeadcountWindow> List()
    {
        var outValue = new List<HeadcountWindow>();
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
public sealed class CachedHeadcountWindowStore(MemoryHeadcountWindowStore inner, HeadcountWindowCache cache, int hits, int misses) : IHeadcountWindowStore
{
    public MemoryHeadcountWindowStore Inner { get; set; } = inner;
    public HeadcountWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountWindow? Find(string code)
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

    public void Save(HeadcountWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount windows. Empty fields match all.</summary>
public sealed record HeadcountWindowQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountWindow item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountWindow> Apply(List<HeadcountWindow> items)
    {
        var matched = Collections.Filter(items, (HeadcountWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount window store.</summary>
public static class HeadcountWindowStore
{
    public static IHeadcountWindowStore NewMemoryHeadcountWindowStore()
    {
        return new MemoryHeadcountWindowStore(new Dictionary<string, HeadcountWindow>(), new List<string>());
    }

    public static IHeadcountWindowStore NewCachedHeadcountWindowStore(MemoryHeadcountWindowStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountWindowStore(inner, HeadcountWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<HeadcountWindow> FindHeadcountWindows(IHeadcountWindowStore store, HeadcountWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
