namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal pool overrides by code.</summary>
public interface ISeasonalPoolOverrideStore
{
    SeasonalPoolOverride? Find(string code);
    void Save(SeasonalPoolOverride item);
    bool Remove(string code);
    List<SeasonalPoolOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPoolOverrideStore(Dictionary<string, SeasonalPoolOverride> items, List<string> order) : ISeasonalPoolOverrideStore
{
    public Dictionary<string, SeasonalPoolOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPoolOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPoolOverride item)
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

    public List<SeasonalPoolOverride> List()
    {
        var outValue = new List<SeasonalPoolOverride>();
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
public sealed class CachedSeasonalPoolOverrideStore(MemorySeasonalPoolOverrideStore inner, SeasonalPoolOverrideCache cache, int hits, int misses) : ISeasonalPoolOverrideStore
{
    public MemorySeasonalPoolOverrideStore Inner { get; set; } = inner;
    public SeasonalPoolOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPoolOverride? Find(string code)
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

    public void Save(SeasonalPoolOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPoolOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal pool overrides. Empty fields match all.</summary>
public sealed record SeasonalPoolOverrideQuery(string Category, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPoolOverride item)
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

    public List<SeasonalPoolOverride> Apply(List<SeasonalPoolOverride> items)
    {
        var matched = Collections.Filter(items, (SeasonalPoolOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal pool override store.</summary>
public static class SeasonalPoolOverrideStore
{
    public static ISeasonalPoolOverrideStore NewMemorySeasonalPoolOverrideStore()
    {
        return new MemorySeasonalPoolOverrideStore(
            new Dictionary<string, SeasonalPoolOverride>(),
            new List<string>());
    }

    public static ISeasonalPoolOverrideStore NewCachedSeasonalPoolOverrideStore(MemorySeasonalPoolOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPoolOverrideStore(inner, SeasonalPoolOverrideCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SeasonalPoolOverride> FindSeasonalPoolOverrides(ISeasonalPoolOverrideStore store, SeasonalPoolOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
