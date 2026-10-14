namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal holiday rollups by code.</summary>
public interface ISeasonalHolidayRollupStore
{
    SeasonalHolidayRollup? Find(string code);
    void Save(SeasonalHolidayRollup item);
    bool Remove(string code);
    List<SeasonalHolidayRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalHolidayRollupStore(Dictionary<string, SeasonalHolidayRollup> items, List<string> order) : ISeasonalHolidayRollupStore
{
    public Dictionary<string, SeasonalHolidayRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalHolidayRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalHolidayRollup item)
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

    public List<SeasonalHolidayRollup> List()
    {
        var outValue = new List<SeasonalHolidayRollup>();
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
public sealed class CachedSeasonalHolidayRollupStore(MemorySeasonalHolidayRollupStore inner, SeasonalHolidayRollupCache cache, int hits, int misses) : ISeasonalHolidayRollupStore
{
    public MemorySeasonalHolidayRollupStore Inner { get; set; } = inner;
    public SeasonalHolidayRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalHolidayRollup? Find(string code)
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

    public void Save(SeasonalHolidayRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalHolidayRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal holiday rollups. Empty fields match all.</summary>
public sealed record SeasonalHolidayRollupQuery(string Team, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalHolidayRollup item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<SeasonalHolidayRollup> Apply(List<SeasonalHolidayRollup> items)
    {
        var matched = Collections.Filter(items, (SeasonalHolidayRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal holiday rollup store.</summary>
public static class SeasonalHolidayRollupStore
{
    public static ISeasonalHolidayRollupStore NewMemorySeasonalHolidayRollupStore()
    {
        return new MemorySeasonalHolidayRollupStore(
            new Dictionary<string, SeasonalHolidayRollup>(),
            new List<string>());
    }

    public static ISeasonalHolidayRollupStore NewCachedSeasonalHolidayRollupStore(MemorySeasonalHolidayRollupStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalHolidayRollupStore(inner, SeasonalHolidayRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalHolidayRollup> FindSeasonalHolidayRollups(ISeasonalHolidayRollupStore store, SeasonalHolidayRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
