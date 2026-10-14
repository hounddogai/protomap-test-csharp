namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position rollups by code.</summary>
public interface IPositionRollupStore
{
    PositionRollup? Find(string code);
    void Save(PositionRollup item);
    bool Remove(string code);
    List<PositionRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionRollupStore(Dictionary<string, PositionRollup> items, List<string> order) : IPositionRollupStore
{
    public Dictionary<string, PositionRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionRollup item)
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

    public List<PositionRollup> List()
    {
        var outValue = new List<PositionRollup>();
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
public sealed class CachedPositionRollupStore(MemoryPositionRollupStore inner, PositionRollupCache cache, int hits, int misses) : IPositionRollupStore
{
    public MemoryPositionRollupStore Inner { get; set; } = inner;
    public PositionRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionRollup? Find(string code)
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

    public void Save(PositionRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position rollups. Empty fields match all.</summary>
public sealed record PositionRollupQuery(string Source, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PositionRollup item)
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

    public List<PositionRollup> Apply(List<PositionRollup> items)
    {
        var matched = Collections.Filter(items, (PositionRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position rollup store.</summary>
public static class PositionRollupStore
{
    public static IPositionRollupStore NewMemoryPositionRollupStore()
    {
        return new MemoryPositionRollupStore(new Dictionary<string, PositionRollup>(), new List<string>());
    }

    public static IPositionRollupStore NewCachedPositionRollupStore(MemoryPositionRollupStore inner, int ttl, IClock clock)
    {
        return new CachedPositionRollupStore(inner, PositionRollupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionRollup> FindPositionRollups(IPositionRollupStore store, PositionRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
