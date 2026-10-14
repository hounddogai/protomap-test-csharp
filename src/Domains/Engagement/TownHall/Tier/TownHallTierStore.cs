namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps town hall tiers by code.</summary>
public interface ITownHallTierStore
{
    TownHallTier? Find(string code);
    void Save(TownHallTier item);
    bool Remove(string code);
    List<TownHallTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTownHallTierStore(Dictionary<string, TownHallTier> items, List<string> order) : ITownHallTierStore
{
    public Dictionary<string, TownHallTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TownHallTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TownHallTier item)
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

    public List<TownHallTier> List()
    {
        var outValue = new List<TownHallTier>();
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
public sealed class CachedTownHallTierStore(MemoryTownHallTierStore inner, TownHallTierCache cache, int hits, int misses) : ITownHallTierStore
{
    public MemoryTownHallTierStore Inner { get; set; } = inner;
    public TownHallTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TownHallTier? Find(string code)
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

    public void Save(TownHallTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TownHallTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects town hall tiers. Empty fields match all.</summary>
public sealed record TownHallTierQuery(string Team, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TownHallTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<TownHallTier> Apply(List<TownHallTier> items)
    {
        var matched = Collections.Filter(items, (TownHallTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of town hall tier store.</summary>
public static class TownHallTierStore
{
    public static ITownHallTierStore NewMemoryTownHallTierStore()
    {
        return new MemoryTownHallTierStore(new Dictionary<string, TownHallTier>(), new List<string>());
    }

    public static ITownHallTierStore NewCachedTownHallTierStore(MemoryTownHallTierStore inner, int ttl, IClock clock)
    {
        return new CachedTownHallTierStore(inner, TownHallTierCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TownHallTier> FindTownHallTiers(ITownHallTierStore store, TownHallTierQuery query)
    {
        return query.Apply(store.List());
    }
}
