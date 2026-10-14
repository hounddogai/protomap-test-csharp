namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional town hall groups by code.</summary>
public interface IRegionalTownHallGroupStore
{
    RegionalTownHallGroup? Find(string code);
    void Save(RegionalTownHallGroup item);
    bool Remove(string code);
    List<RegionalTownHallGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTownHallGroupStore(Dictionary<string, RegionalTownHallGroup> items, List<string> order) : IRegionalTownHallGroupStore
{
    public Dictionary<string, RegionalTownHallGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTownHallGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTownHallGroup item)
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

    public List<RegionalTownHallGroup> List()
    {
        var outValue = new List<RegionalTownHallGroup>();
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
public sealed class CachedRegionalTownHallGroupStore(MemoryRegionalTownHallGroupStore inner, RegionalTownHallGroupCache cache, int hits, int misses) : IRegionalTownHallGroupStore
{
    public MemoryRegionalTownHallGroupStore Inner { get; set; } = inner;
    public RegionalTownHallGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTownHallGroup? Find(string code)
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

    public void Save(RegionalTownHallGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTownHallGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional town hall groups. Empty fields match all.</summary>
public sealed record RegionalTownHallGroupQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTownHallGroup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalTownHallGroup> Apply(List<RegionalTownHallGroup> items)
    {
        var matched = Collections.Filter(items, (RegionalTownHallGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional town hall group store.</summary>
public static class RegionalTownHallGroupStore
{
    public static IRegionalTownHallGroupStore NewMemoryRegionalTownHallGroupStore()
    {
        return new MemoryRegionalTownHallGroupStore(
            new Dictionary<string, RegionalTownHallGroup>(),
            new List<string>());
    }

    public static IRegionalTownHallGroupStore NewCachedRegionalTownHallGroupStore(MemoryRegionalTownHallGroupStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTownHallGroupStore(inner, RegionalTownHallGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<RegionalTownHallGroup> FindRegionalTownHallGroups(IRegionalTownHallGroupStore store, RegionalTownHallGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
