namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position targets by code.</summary>
public interface IPositionTargetStore
{
    PositionTarget? Find(string code);
    void Save(PositionTarget item);
    bool Remove(string code);
    List<PositionTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionTargetStore(Dictionary<string, PositionTarget> items, List<string> order) : IPositionTargetStore
{
    public Dictionary<string, PositionTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionTarget item)
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

    public List<PositionTarget> List()
    {
        var outValue = new List<PositionTarget>();
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
public sealed class CachedPositionTargetStore(MemoryPositionTargetStore inner, PositionTargetCache cache, int hits, int misses) : IPositionTargetStore
{
    public MemoryPositionTargetStore Inner { get; set; } = inner;
    public PositionTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionTarget? Find(string code)
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

    public void Save(PositionTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position targets. Empty fields match all.</summary>
public sealed record PositionTargetQuery(string Department, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PositionTarget item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<PositionTarget> Apply(List<PositionTarget> items)
    {
        var matched = Collections.Filter(items, (PositionTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position target store.</summary>
public static class PositionTargetStore
{
    public static IPositionTargetStore NewMemoryPositionTargetStore()
    {
        return new MemoryPositionTargetStore(new Dictionary<string, PositionTarget>(), new List<string>());
    }

    public static IPositionTargetStore NewCachedPositionTargetStore(MemoryPositionTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPositionTargetStore(inner, PositionTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PositionTarget> FindPositionTargets(IPositionTargetStore store, PositionTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
