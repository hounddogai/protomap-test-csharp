namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor targets by code.</summary>
public interface IVisitorTargetStore
{
    VisitorTarget? Find(string code);
    void Save(VisitorTarget item);
    bool Remove(string code);
    List<VisitorTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorTargetStore(Dictionary<string, VisitorTarget> items, List<string> order) : IVisitorTargetStore
{
    public Dictionary<string, VisitorTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorTarget item)
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

    public List<VisitorTarget> List()
    {
        var outValue = new List<VisitorTarget>();
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
public sealed class CachedVisitorTargetStore(MemoryVisitorTargetStore inner, VisitorTargetCache cache, int hits, int misses) : IVisitorTargetStore
{
    public MemoryVisitorTargetStore Inner { get; set; } = inner;
    public VisitorTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorTarget? Find(string code)
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

    public void Save(VisitorTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor targets. Empty fields match all.</summary>
public sealed record VisitorTargetQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorTarget item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorTarget> Apply(List<VisitorTarget> items)
    {
        var matched = Collections.Filter(items, (VisitorTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor target store.</summary>
public static class VisitorTargetStore
{
    public static IVisitorTargetStore NewMemoryVisitorTargetStore()
    {
        return new MemoryVisitorTargetStore(new Dictionary<string, VisitorTarget>(), new List<string>());
    }

    public static IVisitorTargetStore NewCachedVisitorTargetStore(MemoryVisitorTargetStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorTargetStore(inner, VisitorTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<VisitorTarget> FindVisitorTargets(IVisitorTargetStore store, VisitorTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
