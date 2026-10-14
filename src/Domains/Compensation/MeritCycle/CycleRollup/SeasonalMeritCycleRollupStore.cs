namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal merit cycle rollups by code.</summary>
public interface ISeasonalMeritCycleRollupStore
{
    SeasonalMeritCycleRollup? Find(string code);
    void Save(SeasonalMeritCycleRollup item);
    bool Remove(string code);
    List<SeasonalMeritCycleRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalMeritCycleRollupStore(Dictionary<string, SeasonalMeritCycleRollup> items, List<string> order) : ISeasonalMeritCycleRollupStore
{
    public Dictionary<string, SeasonalMeritCycleRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalMeritCycleRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalMeritCycleRollup item)
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

    public List<SeasonalMeritCycleRollup> List()
    {
        var outValue = new List<SeasonalMeritCycleRollup>();
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
public sealed class CachedSeasonalMeritCycleRollupStore(MemorySeasonalMeritCycleRollupStore inner, SeasonalMeritCycleRollupCache cache, int hits, int misses) : ISeasonalMeritCycleRollupStore
{
    public MemorySeasonalMeritCycleRollupStore Inner { get; set; } = inner;
    public SeasonalMeritCycleRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalMeritCycleRollup? Find(string code)
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

    public void Save(SeasonalMeritCycleRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalMeritCycleRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal merit cycle rollups. Empty fields match all.</summary>
public sealed record SeasonalMeritCycleRollupQuery(string Title, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalMeritCycleRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalMeritCycleRollup> Apply(List<SeasonalMeritCycleRollup> items)
    {
        var matched = Collections.Filter(items, (SeasonalMeritCycleRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal merit cycle rollup store.</summary>
public static class SeasonalMeritCycleRollupStore
{
    public static ISeasonalMeritCycleRollupStore NewMemorySeasonalMeritCycleRollupStore()
    {
        return new MemorySeasonalMeritCycleRollupStore(
            new Dictionary<string, SeasonalMeritCycleRollup>(),
            new List<string>());
    }

    public static ISeasonalMeritCycleRollupStore NewCachedSeasonalMeritCycleRollupStore(MemorySeasonalMeritCycleRollupStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalMeritCycleRollupStore(
            inner,
            SeasonalMeritCycleRollupCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalMeritCycleRollup> FindSeasonalMeritCycleRollups(ISeasonalMeritCycleRollupStore store, SeasonalMeritCycleRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
