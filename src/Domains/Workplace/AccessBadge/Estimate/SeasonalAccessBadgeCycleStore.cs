namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal access badge cycles by code.</summary>
public interface ISeasonalAccessBadgeCycleStore
{
    SeasonalAccessBadgeCycle? Find(string code);
    void Save(SeasonalAccessBadgeCycle item);
    bool Remove(string code);
    List<SeasonalAccessBadgeCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAccessBadgeCycleStore(Dictionary<string, SeasonalAccessBadgeCycle> items, List<string> order) : ISeasonalAccessBadgeCycleStore
{
    public Dictionary<string, SeasonalAccessBadgeCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAccessBadgeCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAccessBadgeCycle item)
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

    public List<SeasonalAccessBadgeCycle> List()
    {
        var outValue = new List<SeasonalAccessBadgeCycle>();
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
public sealed class CachedSeasonalAccessBadgeCycleStore(MemorySeasonalAccessBadgeCycleStore inner, SeasonalAccessBadgeCycleCache cache, int hits, int misses) : ISeasonalAccessBadgeCycleStore
{
    public MemorySeasonalAccessBadgeCycleStore Inner { get; set; } = inner;
    public SeasonalAccessBadgeCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAccessBadgeCycle? Find(string code)
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

    public void Save(SeasonalAccessBadgeCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAccessBadgeCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal access badge cycles. Empty fields match all.</summary>
public sealed record SeasonalAccessBadgeCycleQuery(string Category, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAccessBadgeCycle item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAccessBadgeCycle> Apply(List<SeasonalAccessBadgeCycle> items)
    {
        var matched = Collections.Filter(items, (SeasonalAccessBadgeCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal access badge cycle store.</summary>
public static class SeasonalAccessBadgeCycleStore
{
    public static ISeasonalAccessBadgeCycleStore NewMemorySeasonalAccessBadgeCycleStore()
    {
        return new MemorySeasonalAccessBadgeCycleStore(
            new Dictionary<string, SeasonalAccessBadgeCycle>(),
            new List<string>());
    }

    public static ISeasonalAccessBadgeCycleStore NewCachedSeasonalAccessBadgeCycleStore(MemorySeasonalAccessBadgeCycleStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAccessBadgeCycleStore(
            inner,
            SeasonalAccessBadgeCycleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAccessBadgeCycle> FindSeasonalAccessBadgeCycles(ISeasonalAccessBadgeCycleStore store, SeasonalAccessBadgeCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
