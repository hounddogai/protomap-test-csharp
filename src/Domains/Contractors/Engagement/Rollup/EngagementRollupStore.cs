namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement rollups by code.</summary>
public interface IEngagementRollupStore
{
    EngagementRollup? Find(string code);
    void Save(EngagementRollup item);
    bool Remove(string code);
    List<EngagementRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementRollupStore(Dictionary<string, EngagementRollup> items, List<string> order) : IEngagementRollupStore
{
    public Dictionary<string, EngagementRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementRollup item)
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

    public List<EngagementRollup> List()
    {
        var outValue = new List<EngagementRollup>();
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
public sealed class CachedEngagementRollupStore(MemoryEngagementRollupStore inner, EngagementRollupCache cache, int hits, int misses) : IEngagementRollupStore
{
    public MemoryEngagementRollupStore Inner { get; set; } = inner;
    public EngagementRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementRollup? Find(string code)
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

    public void Save(EngagementRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement rollups. Empty fields match all.</summary>
public sealed record EngagementRollupQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementRollup> Apply(List<EngagementRollup> items)
    {
        var matched = Collections.Filter(items, (EngagementRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement rollup store.</summary>
public static class EngagementRollupStore
{
    public static IEngagementRollupStore NewMemoryEngagementRollupStore()
    {
        return new MemoryEngagementRollupStore(new Dictionary<string, EngagementRollup>(), new List<string>());
    }

    public static IEngagementRollupStore NewCachedEngagementRollupStore(MemoryEngagementRollupStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementRollupStore(inner, EngagementRollupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EngagementRollup> FindEngagementRollups(IEngagementRollupStore store, EngagementRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
