namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor cycles by code.</summary>
public interface IVisitorCycleStore
{
    VisitorCycle? Find(string code);
    void Save(VisitorCycle item);
    bool Remove(string code);
    List<VisitorCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorCycleStore(Dictionary<string, VisitorCycle> items, List<string> order) : IVisitorCycleStore
{
    public Dictionary<string, VisitorCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorCycle item)
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

    public List<VisitorCycle> List()
    {
        var outValue = new List<VisitorCycle>();
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
public sealed class CachedVisitorCycleStore(MemoryVisitorCycleStore inner, VisitorCycleCache cache, int hits, int misses) : IVisitorCycleStore
{
    public MemoryVisitorCycleStore Inner { get; set; } = inner;
    public VisitorCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorCycle? Find(string code)
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

    public void Save(VisitorCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor cycles. Empty fields match all.</summary>
public sealed record VisitorCycleQuery(string Name, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorCycle item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorCycle> Apply(List<VisitorCycle> items)
    {
        var matched = Collections.Filter(items, (VisitorCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor cycle store.</summary>
public static class VisitorCycleStore
{
    public static IVisitorCycleStore NewMemoryVisitorCycleStore()
    {
        return new MemoryVisitorCycleStore(new Dictionary<string, VisitorCycle>(), new List<string>());
    }

    public static IVisitorCycleStore NewCachedVisitorCycleStore(MemoryVisitorCycleStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorCycleStore(inner, VisitorCycleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorCycle> FindVisitorCycles(IVisitorCycleStore store, VisitorCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
