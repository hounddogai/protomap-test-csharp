namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle windows by code.</summary>
public interface IMeritCycleWindowStore
{
    MeritCycleWindow? Find(string code);
    void Save(MeritCycleWindow item);
    bool Remove(string code);
    List<MeritCycleWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleWindowStore(Dictionary<string, MeritCycleWindow> items, List<string> order) : IMeritCycleWindowStore
{
    public Dictionary<string, MeritCycleWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleWindow item)
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

    public List<MeritCycleWindow> List()
    {
        var outValue = new List<MeritCycleWindow>();
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
public sealed class CachedMeritCycleWindowStore(MemoryMeritCycleWindowStore inner, MeritCycleWindowCache cache, int hits, int misses) : IMeritCycleWindowStore
{
    public MemoryMeritCycleWindowStore Inner { get; set; } = inner;
    public MeritCycleWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleWindow? Find(string code)
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

    public void Save(MeritCycleWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle windows. Empty fields match all.</summary>
public sealed record MeritCycleWindowQuery(string Name, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<MeritCycleWindow> Apply(List<MeritCycleWindow> items)
    {
        var matched = Collections.Filter(items, (MeritCycleWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle window store.</summary>
public static class MeritCycleWindowStore
{
    public static IMeritCycleWindowStore NewMemoryMeritCycleWindowStore()
    {
        return new MemoryMeritCycleWindowStore(new Dictionary<string, MeritCycleWindow>(), new List<string>());
    }

    public static IMeritCycleWindowStore NewCachedMeritCycleWindowStore(MemoryMeritCycleWindowStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleWindowStore(inner, MeritCycleWindowCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<MeritCycleWindow> FindMeritCycleWindows(IMeritCycleWindowStore store, MeritCycleWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
