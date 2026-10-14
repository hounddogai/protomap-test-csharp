namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal engagement rollups by code.</summary>
public interface ISeasonalEngagementRollupStore
{
    SeasonalEngagementRollup? Find(string code);
    void Save(SeasonalEngagementRollup item);
    bool Remove(string code);
    List<SeasonalEngagementRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalEngagementRollupStore(Dictionary<string, SeasonalEngagementRollup> items, List<string> order) : ISeasonalEngagementRollupStore
{
    public Dictionary<string, SeasonalEngagementRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalEngagementRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalEngagementRollup item)
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

    public List<SeasonalEngagementRollup> List()
    {
        var outValue = new List<SeasonalEngagementRollup>();
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
public sealed class CachedSeasonalEngagementRollupStore(MemorySeasonalEngagementRollupStore inner, SeasonalEngagementRollupCache cache, int hits, int misses) : ISeasonalEngagementRollupStore
{
    public MemorySeasonalEngagementRollupStore Inner { get; set; } = inner;
    public SeasonalEngagementRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalEngagementRollup? Find(string code)
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

    public void Save(SeasonalEngagementRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalEngagementRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal engagement rollups. Empty fields match all.</summary>
public sealed record SeasonalEngagementRollupQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalEngagementRollup item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<SeasonalEngagementRollup> Apply(List<SeasonalEngagementRollup> items)
    {
        var matched = Collections.Filter(items, (SeasonalEngagementRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal engagement rollup store.</summary>
public static class SeasonalEngagementRollupStore
{
    public static ISeasonalEngagementRollupStore NewMemorySeasonalEngagementRollupStore()
    {
        return new MemorySeasonalEngagementRollupStore(
            new Dictionary<string, SeasonalEngagementRollup>(),
            new List<string>());
    }

    public static ISeasonalEngagementRollupStore NewCachedSeasonalEngagementRollupStore(MemorySeasonalEngagementRollupStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalEngagementRollupStore(
            inner,
            SeasonalEngagementRollupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalEngagementRollup> FindSeasonalEngagementRollups(ISeasonalEngagementRollupStore store, SeasonalEngagementRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
