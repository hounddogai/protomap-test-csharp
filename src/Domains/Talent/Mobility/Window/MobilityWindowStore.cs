namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility windows by code.</summary>
public interface IMobilityWindowStore
{
    MobilityWindow? Find(string code);
    void Save(MobilityWindow item);
    bool Remove(string code);
    List<MobilityWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityWindowStore(Dictionary<string, MobilityWindow> items, List<string> order) : IMobilityWindowStore
{
    public Dictionary<string, MobilityWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityWindow item)
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

    public List<MobilityWindow> List()
    {
        var outValue = new List<MobilityWindow>();
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
public sealed class CachedMobilityWindowStore(MemoryMobilityWindowStore inner, MobilityWindowCache cache, int hits, int misses) : IMobilityWindowStore
{
    public MemoryMobilityWindowStore Inner { get; set; } = inner;
    public MobilityWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityWindow? Find(string code)
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

    public void Save(MobilityWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility windows. Empty fields match all.</summary>
public sealed record MobilityWindowQuery(string Segment, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityWindow item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<MobilityWindow> Apply(List<MobilityWindow> items)
    {
        var matched = Collections.Filter(items, (MobilityWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility window store.</summary>
public static class MobilityWindowStore
{
    public static IMobilityWindowStore NewMemoryMobilityWindowStore()
    {
        return new MemoryMobilityWindowStore(new Dictionary<string, MobilityWindow>(), new List<string>());
    }

    public static IMobilityWindowStore NewCachedMobilityWindowStore(MemoryMobilityWindowStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityWindowStore(inner, MobilityWindowCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MobilityWindow> FindMobilityWindows(IMobilityWindowStore store, MobilityWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
