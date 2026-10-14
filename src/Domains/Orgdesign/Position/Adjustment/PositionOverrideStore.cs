namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position overrides by code.</summary>
public interface IPositionOverrideStore
{
    PositionOverride? Find(string code);
    void Save(PositionOverride item);
    bool Remove(string code);
    List<PositionOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionOverrideStore(Dictionary<string, PositionOverride> items, List<string> order) : IPositionOverrideStore
{
    public Dictionary<string, PositionOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionOverride item)
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

    public List<PositionOverride> List()
    {
        var outValue = new List<PositionOverride>();
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
public sealed class CachedPositionOverrideStore(MemoryPositionOverrideStore inner, PositionOverrideCache cache, int hits, int misses) : IPositionOverrideStore
{
    public MemoryPositionOverrideStore Inner { get; set; } = inner;
    public PositionOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionOverride? Find(string code)
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

    public void Save(PositionOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position overrides. Empty fields match all.</summary>
public sealed record PositionOverrideQuery(string Name, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PositionOverride item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<PositionOverride> Apply(List<PositionOverride> items)
    {
        var matched = Collections.Filter(items, (PositionOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position override store.</summary>
public static class PositionOverrideStore
{
    public static IPositionOverrideStore NewMemoryPositionOverrideStore()
    {
        return new MemoryPositionOverrideStore(new Dictionary<string, PositionOverride>(), new List<string>());
    }

    public static IPositionOverrideStore NewCachedPositionOverrideStore(MemoryPositionOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPositionOverrideStore(inner, PositionOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PositionOverride> FindPositionOverrides(IPositionOverrideStore store, PositionOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
