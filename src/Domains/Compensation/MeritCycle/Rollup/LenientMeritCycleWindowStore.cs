namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient merit cycle windows by code.</summary>
public interface ILenientMeritCycleWindowStore
{
    LenientMeritCycleWindow? Find(string code);
    void Save(LenientMeritCycleWindow item);
    bool Remove(string code);
    List<LenientMeritCycleWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientMeritCycleWindowStore(Dictionary<string, LenientMeritCycleWindow> items, List<string> order) : ILenientMeritCycleWindowStore
{
    public Dictionary<string, LenientMeritCycleWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientMeritCycleWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientMeritCycleWindow item)
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

    public List<LenientMeritCycleWindow> List()
    {
        var outValue = new List<LenientMeritCycleWindow>();
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
public sealed class CachedLenientMeritCycleWindowStore(MemoryLenientMeritCycleWindowStore inner, LenientMeritCycleWindowCache cache, int hits, int misses) : ILenientMeritCycleWindowStore
{
    public MemoryLenientMeritCycleWindowStore Inner { get; set; } = inner;
    public LenientMeritCycleWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientMeritCycleWindow? Find(string code)
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

    public void Save(LenientMeritCycleWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientMeritCycleWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient merit cycle windows. Empty fields match all.</summary>
public sealed record LenientMeritCycleWindowQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(LenientMeritCycleWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientMeritCycleWindow> Apply(List<LenientMeritCycleWindow> items)
    {
        var matched = Collections.Filter(items, (LenientMeritCycleWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient merit cycle window store.</summary>
public static class LenientMeritCycleWindowStore
{
    public static ILenientMeritCycleWindowStore NewMemoryLenientMeritCycleWindowStore()
    {
        return new MemoryLenientMeritCycleWindowStore(
            new Dictionary<string, LenientMeritCycleWindow>(),
            new List<string>());
    }

    public static ILenientMeritCycleWindowStore NewCachedLenientMeritCycleWindowStore(MemoryLenientMeritCycleWindowStore inner, int ttl, IClock clock)
    {
        return new CachedLenientMeritCycleWindowStore(
            inner,
            LenientMeritCycleWindowCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientMeritCycleWindow> FindLenientMeritCycleWindows(ILenientMeritCycleWindowStore store, LenientMeritCycleWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
