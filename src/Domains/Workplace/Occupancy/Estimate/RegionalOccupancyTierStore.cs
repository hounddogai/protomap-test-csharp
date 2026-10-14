namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional occupancy tiers by code.</summary>
public interface IRegionalOccupancyTierStore
{
    RegionalOccupancyTier? Find(string code);
    void Save(RegionalOccupancyTier item);
    bool Remove(string code);
    List<RegionalOccupancyTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalOccupancyTierStore(Dictionary<string, RegionalOccupancyTier> items, List<string> order) : IRegionalOccupancyTierStore
{
    public Dictionary<string, RegionalOccupancyTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalOccupancyTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalOccupancyTier item)
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

    public List<RegionalOccupancyTier> List()
    {
        var outValue = new List<RegionalOccupancyTier>();
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
public sealed class CachedRegionalOccupancyTierStore(MemoryRegionalOccupancyTierStore inner, RegionalOccupancyTierCache cache, int hits, int misses) : IRegionalOccupancyTierStore
{
    public MemoryRegionalOccupancyTierStore Inner { get; set; } = inner;
    public RegionalOccupancyTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalOccupancyTier? Find(string code)
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

    public void Save(RegionalOccupancyTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalOccupancyTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional occupancy tiers. Empty fields match all.</summary>
public sealed record RegionalOccupancyTierQuery(string Team, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalOccupancyTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalOccupancyTier> Apply(List<RegionalOccupancyTier> items)
    {
        var matched = Collections.Filter(items, (RegionalOccupancyTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional occupancy tier store.</summary>
public static class RegionalOccupancyTierStore
{
    public static IRegionalOccupancyTierStore NewMemoryRegionalOccupancyTierStore()
    {
        return new MemoryRegionalOccupancyTierStore(
            new Dictionary<string, RegionalOccupancyTier>(),
            new List<string>());
    }

    public static IRegionalOccupancyTierStore NewCachedRegionalOccupancyTierStore(MemoryRegionalOccupancyTierStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalOccupancyTierStore(inner, RegionalOccupancyTierCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RegionalOccupancyTier> FindRegionalOccupancyTiers(IRegionalOccupancyTierStore store, RegionalOccupancyTierQuery query)
    {
        return query.Apply(store.List());
    }
}
