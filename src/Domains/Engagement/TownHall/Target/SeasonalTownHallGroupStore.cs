namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal town hall groups by code.</summary>
public interface ISeasonalTownHallGroupStore
{
    SeasonalTownHallGroup? Find(string code);
    void Save(SeasonalTownHallGroup item);
    bool Remove(string code);
    List<SeasonalTownHallGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalTownHallGroupStore(Dictionary<string, SeasonalTownHallGroup> items, List<string> order) : ISeasonalTownHallGroupStore
{
    public Dictionary<string, SeasonalTownHallGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalTownHallGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalTownHallGroup item)
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

    public List<SeasonalTownHallGroup> List()
    {
        var outValue = new List<SeasonalTownHallGroup>();
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
public sealed class CachedSeasonalTownHallGroupStore(MemorySeasonalTownHallGroupStore inner, SeasonalTownHallGroupCache cache, int hits, int misses) : ISeasonalTownHallGroupStore
{
    public MemorySeasonalTownHallGroupStore Inner { get; set; } = inner;
    public SeasonalTownHallGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalTownHallGroup? Find(string code)
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

    public void Save(SeasonalTownHallGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalTownHallGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal town hall groups. Empty fields match all.</summary>
public sealed record SeasonalTownHallGroupQuery(string Locale, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalTownHallGroup item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalTownHallGroup> Apply(List<SeasonalTownHallGroup> items)
    {
        var matched = Collections.Filter(items, (SeasonalTownHallGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal town hall group store.</summary>
public static class SeasonalTownHallGroupStore
{
    public static ISeasonalTownHallGroupStore NewMemorySeasonalTownHallGroupStore()
    {
        return new MemorySeasonalTownHallGroupStore(
            new Dictionary<string, SeasonalTownHallGroup>(),
            new List<string>());
    }

    public static ISeasonalTownHallGroupStore NewCachedSeasonalTownHallGroupStore(MemorySeasonalTownHallGroupStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalTownHallGroupStore(inner, SeasonalTownHallGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SeasonalTownHallGroup> FindSeasonalTownHallGroups(ISeasonalTownHallGroupStore store, SeasonalTownHallGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
