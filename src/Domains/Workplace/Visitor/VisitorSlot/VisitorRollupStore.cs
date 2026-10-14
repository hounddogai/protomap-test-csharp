namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor rollups by code.</summary>
public interface IVisitorRollupStore
{
    VisitorRollup? Find(string code);
    void Save(VisitorRollup item);
    bool Remove(string code);
    List<VisitorRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorRollupStore(Dictionary<string, VisitorRollup> items, List<string> order) : IVisitorRollupStore
{
    public Dictionary<string, VisitorRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorRollup item)
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

    public List<VisitorRollup> List()
    {
        var outValue = new List<VisitorRollup>();
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
public sealed class CachedVisitorRollupStore(MemoryVisitorRollupStore inner, VisitorRollupCache cache, int hits, int misses) : IVisitorRollupStore
{
    public MemoryVisitorRollupStore Inner { get; set; } = inner;
    public VisitorRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorRollup? Find(string code)
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

    public void Save(VisitorRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor rollups. Empty fields match all.</summary>
public sealed record VisitorRollupQuery(string Unit, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorRollup item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorRollup> Apply(List<VisitorRollup> items)
    {
        var matched = Collections.Filter(items, (VisitorRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor rollup store.</summary>
public static class VisitorRollupStore
{
    public static IVisitorRollupStore NewMemoryVisitorRollupStore()
    {
        return new MemoryVisitorRollupStore(new Dictionary<string, VisitorRollup>(), new List<string>());
    }

    public static IVisitorRollupStore NewCachedVisitorRollupStore(MemoryVisitorRollupStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorRollupStore(inner, VisitorRollupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorRollup> FindVisitorRollups(IVisitorRollupStore store, VisitorRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
