namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position cycles by code.</summary>
public interface IPositionCycleStore
{
    PositionCycle? Find(string code);
    void Save(PositionCycle item);
    bool Remove(string code);
    List<PositionCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionCycleStore(Dictionary<string, PositionCycle> items, List<string> order) : IPositionCycleStore
{
    public Dictionary<string, PositionCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionCycle item)
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

    public List<PositionCycle> List()
    {
        var outValue = new List<PositionCycle>();
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
public sealed class CachedPositionCycleStore(MemoryPositionCycleStore inner, PositionCycleCache cache, int hits, int misses) : IPositionCycleStore
{
    public MemoryPositionCycleStore Inner { get; set; } = inner;
    public PositionCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionCycle? Find(string code)
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

    public void Save(PositionCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position cycles. Empty fields match all.</summary>
public sealed record PositionCycleQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(PositionCycle item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionCycle> Apply(List<PositionCycle> items)
    {
        var matched = Collections.Filter(items, (PositionCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position cycle store.</summary>
public static class PositionCycleStore
{
    public static IPositionCycleStore NewMemoryPositionCycleStore()
    {
        return new MemoryPositionCycleStore(new Dictionary<string, PositionCycle>(), new List<string>());
    }

    public static IPositionCycleStore NewCachedPositionCycleStore(MemoryPositionCycleStore inner, int ttl, IClock clock)
    {
        return new CachedPositionCycleStore(inner, PositionCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PositionCycle> FindPositionCycles(IPositionCycleStore store, PositionCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
