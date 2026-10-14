namespace Acme.Workforce.Orgdesign.Position.PositionEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position windows by code.</summary>
public interface IPositionWindowStore
{
    PositionWindow? Find(string code);
    void Save(PositionWindow item);
    bool Remove(string code);
    List<PositionWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionWindowStore(Dictionary<string, PositionWindow> items, List<string> order) : IPositionWindowStore
{
    public Dictionary<string, PositionWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionWindow item)
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

    public List<PositionWindow> List()
    {
        var outValue = new List<PositionWindow>();
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
public sealed class CachedPositionWindowStore(MemoryPositionWindowStore inner, PositionWindowCache cache, int hits, int misses) : IPositionWindowStore
{
    public MemoryPositionWindowStore Inner { get; set; } = inner;
    public PositionWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionWindow? Find(string code)
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

    public void Save(PositionWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position windows. Empty fields match all.</summary>
public sealed record PositionWindowQuery(string Unit, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PositionWindow item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionWindow> Apply(List<PositionWindow> items)
    {
        var matched = Collections.Filter(items, (PositionWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position window store.</summary>
public static class PositionWindowStore
{
    public static IPositionWindowStore NewMemoryPositionWindowStore()
    {
        return new MemoryPositionWindowStore(new Dictionary<string, PositionWindow>(), new List<string>());
    }

    public static IPositionWindowStore NewCachedPositionWindowStore(MemoryPositionWindowStore inner, int ttl, IClock clock)
    {
        return new CachedPositionWindowStore(inner, PositionWindowCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionWindow> FindPositionWindows(IPositionWindowStore store, PositionWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
