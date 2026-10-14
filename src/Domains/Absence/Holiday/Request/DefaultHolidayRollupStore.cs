namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default holiday rollups by code.</summary>
public interface IDefaultHolidayRollupStore
{
    DefaultHolidayRollup? Find(string code);
    void Save(DefaultHolidayRollup item);
    bool Remove(string code);
    List<DefaultHolidayRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultHolidayRollupStore(Dictionary<string, DefaultHolidayRollup> items, List<string> order) : IDefaultHolidayRollupStore
{
    public Dictionary<string, DefaultHolidayRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultHolidayRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultHolidayRollup item)
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

    public List<DefaultHolidayRollup> List()
    {
        var outValue = new List<DefaultHolidayRollup>();
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
public sealed class CachedDefaultHolidayRollupStore(MemoryDefaultHolidayRollupStore inner, DefaultHolidayRollupCache cache, int hits, int misses) : IDefaultHolidayRollupStore
{
    public MemoryDefaultHolidayRollupStore Inner { get; set; } = inner;
    public DefaultHolidayRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultHolidayRollup? Find(string code)
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

    public void Save(DefaultHolidayRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultHolidayRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default holiday rollups. Empty fields match all.</summary>
public sealed record DefaultHolidayRollupQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultHolidayRollup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultHolidayRollup> Apply(List<DefaultHolidayRollup> items)
    {
        var matched = Collections.Filter(items, (DefaultHolidayRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default holiday rollup store.</summary>
public static class DefaultHolidayRollupStore
{
    public static IDefaultHolidayRollupStore NewMemoryDefaultHolidayRollupStore()
    {
        return new MemoryDefaultHolidayRollupStore(
            new Dictionary<string, DefaultHolidayRollup>(),
            new List<string>());
    }

    public static IDefaultHolidayRollupStore NewCachedDefaultHolidayRollupStore(MemoryDefaultHolidayRollupStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultHolidayRollupStore(inner, DefaultHolidayRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultHolidayRollup> FindDefaultHolidayRollups(IDefaultHolidayRollupStore store, DefaultHolidayRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
